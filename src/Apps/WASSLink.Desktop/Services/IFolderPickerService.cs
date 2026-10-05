using System.Threading.Tasks;

namespace WASSLink.Desktop.Services;

/// <summary>
/// Contrato para el servicio de selección de carpeta en la UI.
/// </summary>
public interface IFolderPickerService
{
    /// <summary>
    /// Abre el diálogo de selección de carpeta y devuelve la ruta elegida,
    /// o null si el usuario cancela.
    /// </summary>
    Task<string?> PickFolderAsync(string? suggestedPath = null);
}
