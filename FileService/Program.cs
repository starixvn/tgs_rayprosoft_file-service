using System;
using System.Threading.Tasks;
using Amazon.S3.Model;
using FileService.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus.DotNetRuntime;

namespace FileService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DotNetRuntimeStatsBuilder
                .Customize()
                .WithContentionStats(sampleRate: SampleEvery.FiveEvents)
                .WithJitStats(sampleRate: SampleEvery.TenEvents)
                .WithGcStats()
                .WithExceptionStats()
                .StartCollecting();

            var host = CreateHostBuilder(args).Build();
            await CreateBucketAsync(host.Services);
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((contxt, builder) =>
                {
                    builder.AddJsonFile("/config/appsettings.json", true);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddTgsLog();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static async Task CreateBucketAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;
            var publicAmazonS3 = sp.GetRequiredService<IPublicAmazonS3>();
            var privateAmazonS3 = sp.GetRequiredService<IPrivateAmazonS3>();
            var settings = sp.GetRequiredService<IOptions<AwsOptions>>().Value;
            var configuration = sp.GetRequiredService<IConfiguration>();

            if (configuration.GetValue<string>(Consts.EnvironmentVariable.Environment) == "Development" &&
                settings != null && !await publicAmazonS3.DoesS3BucketExistAsync(settings.Public.Bucket))
            {
                var putBucketRequest =
                    new PutBucketRequest
                    {
                        BucketName = settings.Public.Bucket,
                        UseClientRegion = true
                    };

                await publicAmazonS3.PutBucketAsync(putBucketRequest);
            }

            if (configuration.GetValue<string>(Consts.EnvironmentVariable.Environment) == "Development" &&
                settings != null && !await privateAmazonS3.DoesS3BucketExistAsync(settings.Private.Bucket))
            {
                var putBucketRequest =
                    new PutBucketRequest
                    {
                        BucketName = settings.Private.Bucket,
                        UseClientRegion = true
                    };

                await privateAmazonS3.PutBucketAsync(putBucketRequest);
            }
        }
    }
}