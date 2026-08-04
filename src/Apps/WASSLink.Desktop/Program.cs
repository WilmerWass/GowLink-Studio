using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WASSLink.Desktop.DependencyInjection;

using WASSLink.Configuration.DependencyInjection;
using WASSLink.Download.DependencyInjection;
using WASSLink.Library.DependencyInjection;
using WASSLink.Player.DependencyInjection;
using WASSLink.Search.DependencyInjection;
using WASSLink.Plugins.DependencyInjection;

namespace WASSLink.Desktop;

sealed class Program
{
    public static IHost Host { get; private set; } = null!;

    public static IServiceProvider Services => Host.Services;

    [STAThread]
    public static void Main(string[] args)
    {
        Host = CreateHostBuilder(args).Build();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }


    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services
                    .AddConfigurationServices()
                    .AddDownloadServices()
                    .AddLibraryServices()
                    .AddPlayerServices()
                    .AddSearchServices()
                    .AddPluginServices()
                    .AddDesktopServices();
            });
    }


    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
