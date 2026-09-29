using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace WASSLink.Desktop.Services;

/// <summary>
/// Implementación de IClipboardService para Avalonia UI 12.1+.
/// Usa la API DataTransfer / IAsyncDataTransfer del nuevo sistema de clipboard.
/// </summary>
public class AvaloniaClipboardService : IClipboardService
{
    private static IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow != null)
        {
            return TopLevel.GetTopLevel(desktop.MainWindow)?.Clipboard;
        }
        return null;
    }

    public async Task<string?> GetTextAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return null;

        // TryGetDataAsync() retorna IAsyncDataTransfer? (sin parámetros en Avalonia 12.1)
        using var data = await clipboard.TryGetDataAsync();
        if (data == null) return null;

        // Usar el método de extensión TryGetTextAsync() de AsyncDataTransferExtensions
        return await data.TryGetTextAsync();
    }

    public async Task SetTextAsync(string text)
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return;

        // Crear DataTransfer con un DataTransferItem de texto
        var transfer = new DataTransfer();
        transfer.Add(DataTransferItem.CreateText(text));
        await clipboard.SetDataAsync(transfer);
    }
}
