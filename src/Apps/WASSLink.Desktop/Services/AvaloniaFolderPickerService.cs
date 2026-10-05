using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace WASSLink.Desktop.Services;

/// <summary>
/// Implementación del selector de carpeta usando la API de Avalonia StorageProvider.
/// </summary>
public class AvaloniaFolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync(string? suggestedPath = null)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var window = desktop.MainWindow;
        if (window is null)
            return null;

        var options = new FolderPickerOpenOptions
        {
            Title = "Seleccionar carpeta de destino",
            AllowMultiple = false
        };

        if (!string.IsNullOrWhiteSpace(suggestedPath))
        {
            try
            {
                var suggested = await window.StorageProvider.TryGetFolderFromPathAsync(suggestedPath);
                if (suggested is not null)
                    options.SuggestedStartLocation = suggested;
            }
            catch
            {
                // Silencioso: si la ruta sugerida no existe, se ignora
            }
        }

        var result = await window.StorageProvider.OpenFolderPickerAsync(options);
        if (result is { Count: > 0 })
            return result[0].TryGetLocalPath();

        return null;
    }
}
