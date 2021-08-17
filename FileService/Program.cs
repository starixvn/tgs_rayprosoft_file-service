using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    }
}