using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using WASSLink.Shared.Diagnostics;
using WASSLink.Shared.Logging;
using WASSLink.Desktop.Views;

namespace WASSLink.Desktop;

public partial class App : Application
{
    private readonly GowLinkLogger _logger = new();

    public override void Initialize()
    {
        _logger.Info("GowLink Studio initializing.");

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _logger.Info("Framework initialization completed.");

        _logger.Info("Diagnostic report generation started.");

        var diagnosticGenerator = new DiagnosticReportGenerator();
        diagnosticGenerator.GenerateReport();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.Info("Resolving desktop MainWindow via DI.");

            desktop.MainWindow = Program.Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();

        _logger.Info("GowLink Studio started successfully.");
    }
}