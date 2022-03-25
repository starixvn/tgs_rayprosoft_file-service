using System;
using Amazon;
using FileService.Core;
using FileService.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static FileServiceConfiguration AddFileServiceCore(
            this IServiceCollection services,
            Action<AwsOptions, IServiceProvider> configure)
        {
            services.AddTransient(typeof(IFileManager<>), typeof(FileManager<>));
            services.AddOptions<AwsOptions>().Configure(configure);

            services.AddSingleton<IPublicAmazonS3>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AwsOptions>>().Value;
                if (settings == null)
                {
                    throw new ArgumentException("can't get aws settings");
                }

                return new PublicAmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey,
                    new()
                    {
                        AuthenticationRegion = settings.Region,
                        UseHttp = settings.UseHttp,
                        ServiceURL = settings.Public.ServiceUrl,
                        ForcePathStyle = true
                    });
            });

            services.AddSingleton<IPrivateAmazonS3>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AwsOptions>>().Value;
                if (settings == null)
                {
                    throw new ArgumentException("can't get aws settings");
                }

                return new PrivateAmazonS3Client(settings.AccessKeyId, settings.SecretAccessKey,
                    new()
                    {
                        AuthenticationRegion = settings.Region,
                        UseHttp = settings.UseHttp,
                        ServiceURL = settings.Private.ServiceUrl,
                        ForcePathStyle = true
                    });
            });

            var configuration = new FileServiceConfiguration(services);

            return configuration;
        }
    }
}