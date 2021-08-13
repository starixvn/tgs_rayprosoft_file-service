using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddFile(this IServiceCollection services, IConfiguration configuration,
            string configSectionName = "FileSetting")
        {
            services.Configure<FileSettings>(configuration.GetSection(configSectionName));

            return services;
        }
    }
}