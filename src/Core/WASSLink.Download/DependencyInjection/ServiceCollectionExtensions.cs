using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using WASSLink.Abstractions;

namespace WASSLink.Download.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDownloadServices(
        this IServiceCollection services)
    {
        // Providers concretos
        services.AddSingleton<YtDlpDownloadService>();

        // HttpClient gestionado por DI (sin usar AddHttpClient, para mantener compatibilidad)
        services.AddSingleton<HttpClient>();
        services.AddSingleton<DirectHttpDownloadService>();

        // Resolver providers en un Composite, preservando el contrato IDownloadProvider
        services.AddSingleton<IDownloadProvider>(sp =>
        {
            var allProviders = new IDownloadProvider[]
            {
                sp.GetRequiredService<YtDlpDownloadService>(),
                sp.GetRequiredService<DirectHttpDownloadService>()
            };

            return new CompositeDownloadProvider(allProviders);
        });

        // El Composite es el único IDownloadProvider inyectado.
        return services;
    }
}
