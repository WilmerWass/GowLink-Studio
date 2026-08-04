using Microsoft.Extensions.DependencyInjection;
using WASSLink.Desktop.ViewModels;
using WASSLink.Desktop.Views;

namespace WASSLink.Desktop.DependencyInjection;

public static class DesktopServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopServices(
        this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
