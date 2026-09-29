using Microsoft.Extensions.DependencyInjection;
using WASSLink.Desktop.Services;
using WASSLink.Desktop.ViewModels;
using WASSLink.Desktop.Views;

namespace WASSLink.Desktop.DependencyInjection;

public static class DesktopServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IClipboardService, AvaloniaClipboardService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
