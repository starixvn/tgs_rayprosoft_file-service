using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core.Configuration;

public class FileServiceConfiguration
{
    public IServiceCollection Services { get; }

    public FileServiceConfiguration(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public FileServiceConfiguration UseRepository<TRepository>() where TRepository : class, IFileMetaRepository
    {
        Services.AddTransient<IFileMetaRepository, TRepository>();

        return this;
    }

    public FileServiceConfiguration ConfigureAwsOptions(Action<AwsOptions, IServiceProvider> configure)
    {
        Services
            .AddOptions<AwsOptions>()
            .Configure(configure);

        return this;
    }
}