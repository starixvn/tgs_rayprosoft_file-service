using System;
using System.IO;
using System.Reflection;
using FileService.DataService;
using FileService.DataService.Model;
using FileService.DataService.Services;
using FileService.Extensions;
using FileService.Middlewares;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoFileMetaRepository;
using Prometheus;

namespace FileService
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DatabaseSettings>(
                _configuration.GetSection("DatabaseSettings"));

            services.AddSingleton<IDatabaseSettings>(provider =>
                provider.GetRequiredService<IOptions<DatabaseSettings>>().Value);

            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IDatabaseSettings>();
                return new MongoClient(settings.ConnectionString);
            });

            services.AddTransient(c =>
                c.GetService<IMongoClient>()?.StartSession());

            services
                .AddNatsMessageQueue(config =>
                {
                    config.ConfigQueueOptions((options, sp) =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();

                        options.Url = config.GetConnectionString("Nats");
                    });
                })
                .AddFile(_configuration)
                .AddAws(_configuration);

            services.AddFileServiceCore()
                .UseMongoRepository((options, sp) =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();

                    options.MongoConnectionString = config.GetValue<string>("DatabaseSettings:ConnectionString");
                });

            services.AddTransient<IMongoContext<StationLetterFile>, MongoContext<StationLetterFile>>();
            services.AddTransient<IMongoContext<PromotionFile>, MongoContext<PromotionFile>>();
            services.AddTransient<IUploadService, UploadService>();
            services.AddTransient<IStationLetterService, StationLetterService>();
            services.AddTransient<IPromotionService, PromotionService>();

            services.AddControllers()
                .AddFluentValidation(c =>
                {
                    c.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                });

            services
                .AddHealthChecks()
                .ForwardToPrometheus();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() {Title = "TGS FileService API", Version = "v1"});

                var xml = "TGS.Apis.FileService.xml";
                var path = Path.Combine(AppContext.BaseDirectory, xml);
                if (File.Exists(path))
                {
                    c.IncludeXmlComments(path);
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/fs/health");

                endpoints.MapMetrics();
            });

            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TGS FileService API"));
            }
        }
    }
}