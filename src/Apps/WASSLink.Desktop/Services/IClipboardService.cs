using System.Threading.Tasks;

namespace WASSLink.Desktop.Services;

/// <summary>
/// Servicio para interactuar de forma abstracta con el portapapeles del sistema operativo.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Obtiene el texto actualmente disponible en el portapapeles.
    /// </summary>
    Task<string?> GetTextAsync();

    /// <summary>
    /// Establece el texto en el portapapeles del sistema.
    /// </summary>
    Task SetTextAsync(string text);
}
