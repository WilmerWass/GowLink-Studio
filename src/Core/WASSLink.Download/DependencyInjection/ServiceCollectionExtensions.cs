using Microsoft.Extensions.DependencyInjection;

namespace WASSLink.Download.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDownloadServices(
        this IServiceCollection services)
    {
        return services;
    }
}
