namespace WASSLink.Abstractions;

/// <summary>
/// Resultado tras finalizar un intento de descarga.
/// </summary>
public record DownloadResult(
    bool Success,
    string? OutputPath = null,
    string? ErrorMessage = null
);
