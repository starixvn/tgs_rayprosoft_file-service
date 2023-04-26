using Tgs.FileService.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileServiceClient(this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configClient)
        => services
            .AddHttpClient<FileServiceClient>(configClient)
            .Services
            .AddTransient<IFileServiceClient>(sp => sp.GetRequiredService<FileServiceClient>());
}