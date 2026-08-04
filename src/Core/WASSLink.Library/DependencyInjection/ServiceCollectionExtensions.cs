using Microsoft.Extensions.DependencyInjection;

namespace WASSLink.Library.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLibraryServices(
        this IServiceCollection services)
    {
        return services;
    }
}
