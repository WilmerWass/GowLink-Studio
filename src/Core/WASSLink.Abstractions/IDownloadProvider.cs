using System;
using System.Threading;
using System.Threading.Tasks;

namespace WASSLink.Abstractions;

/// <summary>
/// Contrato base para proveedores de descarga en WASSLink Studio.
/// </summary>
public interface IDownloadProvider
{
    /// <summary>
    /// Nombre identificador del proveedor (por ejemplo: "yt-dlp", "HTTP", "Torrent").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Determina si este proveedor puede gestionar la URL especificada.
    /// </summary>
    /// <param name="url">URL de origen del contenido.</param>
    /// <returns>True si el proveedor soporta la URL; de lo contrario, false.</returns>
    bool CanHandle(string url);

    /// <summary>
    /// Ejecuta la descarga solicitada informando el progreso de manera asíncrona.
    /// </summary>
    /// <param name="request">Información de la solicitud de descarga.</param>
    /// <param name="progress">Receptor de actualizaciones de progreso.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la descarga.</returns>
    Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
