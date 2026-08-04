using Microsoft.Extensions.DependencyInjection;

namespace WASSLink.Search.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSearchServices(
        this IServiceCollection services)
    {
        return services;
    }
}
