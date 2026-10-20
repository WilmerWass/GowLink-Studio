using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WASSLink.Download;

/// <summary>
/// Resuelve una página pública de MediaFire hasta la URL directa del archivo.
/// </summary>
public static class MediaFireDownloadResolver
{
    private static readonly Regex DownloadButtonRegex = new(
            @"<a\b(?=[^>]*\bid=""downloadButton"")(?=[^>]*\bhref=""(?<url>https://download[^""]+)"")[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FileNameRegex = new(
        @"<div class=""filename""[^>]*>\s*(?<name>.*?)\s*</div>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    public static bool IsMediaFirePage(string url)
    {
        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) &&
               uri.Host.EndsWith("mediafire.com", StringComparison.OrdinalIgnoreCase) &&
               uri.AbsolutePath.Contains("/file", StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<(Uri DownloadUri, string? FileName)> ResolveAsync(
        HttpClient httpClient,
        string pageUrl,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(pageUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        var match = DownloadButtonRegex.Match(html);
        if (!match.Success || !Uri.TryCreate(WebUtility.HtmlDecode(match.Groups["url"].Value), UriKind.Absolute, out var downloadUri))
            throw new InvalidOperationException("MediaFire no publicó un enlace directo de descarga.");

        var nameMatch = FileNameRegex.Match(html);
        var fileName = nameMatch.Success
            ? WebUtility.HtmlDecode(nameMatch.Groups["name"].Value).Trim()
            : null;

        return (downloadUri, string.IsNullOrWhiteSpace(fileName) ? null : fileName);
    }
}
