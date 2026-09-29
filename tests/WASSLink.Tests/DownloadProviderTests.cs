using System;
using Microsoft.Extensions.DependencyInjection;
using WASSLink.Abstractions;
using WASSLink.Download;
using WASSLink.Download.DependencyInjection;
using Xunit;

namespace WASSLink.Tests;

public class DownloadProviderTests
{
    [Fact]
    public void YtDlpDownloadService_Implements_IDownloadProvider()
    {
        var service = new YtDlpDownloadService();
        Assert.IsAssignableFrom<IDownloadProvider>(service);
        Assert.Equal("yt-dlp", service.Name);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", true)]
    [InlineData("http://youtube.com/watch?v=test", true)]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", true)]
    [InlineData("https://soundcloud.com/artist/track", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("not-a-url", false)]
    [InlineData("ftp://invalid.com", false)]
    public void CanHandle_ValidatesUrlsCorrectly(string url, bool expected)
    {
        var service = new YtDlpDownloadService();
        var result = service.CanHandle(url);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DownloadRequest_DefaultsAndProperties_AreCorrect()
    {
        var request = new DownloadRequest(
            Url: "https://www.youtube.com/watch?v=123",
            OutputPath: "C:\\Downloads"
        );

        Assert.Equal("https://www.youtube.com/watch?v=123", request.Url);
        Assert.Equal("C:\\Downloads", request.OutputPath);
        Assert.Equal(DownloadMediaType.Audio, request.MediaType);
        Assert.Equal("mp3", request.Format);

        var videoRequest = new DownloadRequest(
            Url: "https://www.youtube.com/watch?v=123",
            OutputPath: "C:\\Downloads",
            MediaType: DownloadMediaType.Video,
            Format: "mp4"
        );

        Assert.Equal(DownloadMediaType.Video, videoRequest.MediaType);
        Assert.Equal("mp4", videoRequest.Format);
    }

    [Fact]
    public void DependencyInjection_Resolves_IDownloadProvider()
    {
        var services = new ServiceCollection();
        services.AddDownloadServices();
        var provider = services.BuildServiceProvider();

        var downloadProvider = provider.GetService<IDownloadProvider>();
        Assert.NotNull(downloadProvider);
        Assert.IsType<YtDlpDownloadService>(downloadProvider);
    }

    [Fact]
    public void ResolveToolPath_FindsYtDlpTool()
    {
        var path = YtDlpDownloadService.ResolveToolPath("yt-dlp", "yt-dlp.exe");
        Assert.True(System.IO.File.Exists(path), $"yt-dlp.exe should be located at: {path}");
    }
}
