using Microsoft.Extensions.DependencyInjection;

namespace WASSLink.Plugins.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPluginServices(
        this IServiceCollection services)
    {
        return services;
    }
}
