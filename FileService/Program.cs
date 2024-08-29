using System.Text;
using System.Text.Json;
using FileService;
using FileService.DataService;
using FileService.DataService.Model;
using FileService.DataService.Services;
using FileService.Extensions;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoFileMetaRepository;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp!;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddSingleton(
        provider => provider.GetRequiredService<IOptions<DatabaseSettings>>().Value)
    .AddOptions<DatabaseSettings>()
    .Configure<IConfiguration>((settings, config) =>
    {
        settings.ConnectionString = config.GetConnectionString("File")!;
    });

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<DatabaseSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services
    .AddTransient(c => c.GetRequiredService<IMongoClient>().StartSession());

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(config =>
    {
        config.ConfigQueueOptions((options, sp) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            options.Url = config.GetConnectionString("Nats")!;
        });
    });

builder.Services
    .AddFile(builder.Configuration);

builder.Services
    .AddFileServiceCore((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        config.GetSection("AWS").Bind(options);
    })
    .UseMongoRepository((options, sp) =>
    {
        var config = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

        options.MongoConnectionString = config.ConnectionString;
    });

builder.Services
    .AddTransient<IMongoContext<StationLetterFile>, MongoContext<StationLetterFile>>()
    .AddTransient<IMongoContext<PromotionFile>, MongoContext<PromotionFile>>()
    .AddTransient<IUploadService, UploadService>()
    .AddTransient<IStationLetterService, StationLetterService>()
    .AddTransient<IPromotionService, PromotionService>();

builder.Services
    .AddFluentValidationAutoValidation(config => config.DisableDataAnnotationsValidation = true)
    .AddFluentValidationClientsideAdapters()
    .AddControllers();

builder.Services
    .AddOptions<OtlpExporterOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        var nodeIp = config.GetValue<string>("NODE_IP");

        if (!string.IsNullOrWhiteSpace(nodeIp))
        {
            options.Endpoint = new($"http://{nodeIp}:4317");
        }

        config.GetSection("Opentelemetry:Tracing").Bind(options);
    });

builder.Services
    .AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
        })
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .Services
    .Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue ||
            !httpContext.Request.Path.StartsWithSegments("/swagger") &&
            !httpContext.Request.Path.StartsWithSegments("/metrics") &&
            !httpContext.Request.Path.StartsWithSegments("/fs/health") &&
            !httpContext.Request.Path.StartsWithSegments("/profiler/") &&
            !new[] { "html", "ico", "png", "jpg" }
                .Any(ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
    .AddProfileViewer();

builder.Services
    .AddSwaggerGen()
    .AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature?.Error != null)
        {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ExceptionHandler");

            logger.LogError(exceptionFeature.Error, exceptionFeature.Error.Message);

            var messageSender = app.Services.GetRequiredService<IMessageSender>();

            await messageSender.PublishAsync(
                    "file.service.error",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ReportErrorMessage(
                        $"{app.Environment}: {exceptionFeature.Error.Message}"))))
                .ConfigureAwait(false);
        }
    }
});

app.UseSwaggerUI();

app.MapControllers();
app.MapProfileViewer();
app.MapSwagger();
app.MapHealthChecks("/fs/health");

app.Run();