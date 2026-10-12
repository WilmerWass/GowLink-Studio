using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using WASSLink.Abstractions;

namespace WASSLink.Download.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDownloadServices(
        this IServiceCollection services)
    {
        services.AddSingleton<DirectHttpDownloadService>();
        services.AddSingleton<YtDlpDownloadService>();
        services.AddSingleton<CompositeDownloadProvider>();
        services.AddSingleton<IDownloadProvider>(provider =>
            provider.GetRequiredService<CompositeDownloadProvider>());

        services.AddSingleton<HttpClient>();
        return services;
    }
}
