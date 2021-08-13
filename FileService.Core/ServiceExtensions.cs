using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAws(this IServiceCollection services,
            IConfiguration configuration, string configSectionName = "AWS")
        {
            services.Configure<AwsOptions>(x => configuration.GetSection(configSectionName));
            //services.AddSingleton(sp => sp.GetRequiredService<IOptions<AwsOptions>>().Value);

            services.AddSingleton<IPublicAmazonS3>(sp =>
            {
                var settings = sp.GetRequiredService<AwsOptions>();
                if (settings == null)
                {
                    throw new ArgumentException("can't get aws settings");
                }

                return new PublicAmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey,
                    new()
                    {
                        UseHttp = settings.UseHttp, ServiceURL = settings.Public.ServiceUrl, ForcePathStyle = true
                    });
            });

            services.AddSingleton<IPrivateAmazonS3>(sp =>
            {
                var settings = sp.GetRequiredService<AwsOptions>();
                if (settings == null)
                {
                    throw new ArgumentException("can't get aws settings");
                }

                return new PrivateAmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey,
                    new()
                    {
                        UseHttp = settings.UseHttp, ServiceURL = settings.Private.ServiceUrl, ForcePathStyle = true
                    });
            });

            return services;
        }
    }
}