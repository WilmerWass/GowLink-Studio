namespace WASSLink.Abstractions;

/// <summary>
/// Representa el estado y progreso en tiempo real de una descarga activa.
/// </summary>
public record DownloadProgress(
    double Percentage,
    string? StatusMessage = null,
    string? Speed = null,
    string? Eta = null
);
