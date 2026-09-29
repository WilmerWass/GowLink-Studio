using Microsoft.Extensions.DependencyInjection;
using WASSLink.Abstractions;

namespace WASSLink.Download.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDownloadServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IDownloadProvider, YtDlpDownloadService>();
        services.AddSingleton<YtDlpDownloadService>();

        return services;
    }
}
