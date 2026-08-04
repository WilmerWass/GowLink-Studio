using Microsoft.Extensions.DependencyInjection;

namespace WASSLink.Player.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerServices(
        this IServiceCollection services)
    {
        return services;
    }
}
