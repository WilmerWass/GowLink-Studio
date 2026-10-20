using Microsoft.Extensions.DependencyInjection;

namespace WASSLink.Configuration.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IUserDataStore, UserDataStore>();
        return services;
    }
}
