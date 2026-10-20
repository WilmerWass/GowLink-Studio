using System;

namespace WASSLink.Desktop.Models;

public sealed class LocalFileItem
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string Category { get; init; }
    public required string Size { get; init; }
    public required DateTime Modified { get; init; }
}
