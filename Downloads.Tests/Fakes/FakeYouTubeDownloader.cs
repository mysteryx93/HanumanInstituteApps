using System.IO;
using System.Net.Http;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads.Tests;

public class FakeYouTubeDownloader : IYouTubeDownloader
{
    private readonly FakeFileSystemService _fileSystem;

    public FakeYouTubeDownloader(FakeFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
    }

    private string _url = "https://www.youtube.com/watch?v=HV9PCGoGbSA";
    private string _title = "Title";
    private StreamManifest? _streamInfo;

    public bool ThrowError { get; set; }

    public void Configure(string url, string title, StreamManifest? streamInfo = null)
    {
        _url = url;
        _title = title;
        _streamInfo = streamInfo;
    }

    public Task<Video> QueryVideoAsync(VideoId videoId, CancellationToken cancellationToken = default) => Task.FromResult(
        new Video(_url, _title, new Author(new ChannelId(), "Author"), new DateTimeOffset(), "description", new TimeSpan(), new Thumbnail[] {}, new List<string>(), new Engagement(0, 0, 0)));

    public Task<StreamManifest> QueryStreamInfoAsync(VideoId videoId, CancellationToken cancellationToken = default) => Task.FromResult(
        _streamInfo ?? new StreamManifest(new List<IStreamInfo>()));

    public async Task DownloadAsync(IStreamInfo streamInfo, string filePath, Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        await using var destination = _fileSystem.File.Create(filePath);
        if (ThrowError)
        {
            throw new HttpRequestException();
        }

        using var stream = new MemoryStream(new byte[streamInfo.Size.Bytes]);
        stream.Write(new byte[streamInfo.Size.Bytes]);
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(
            destination,
            progressCallback != null ? new Progress<double>(progressCallback) : null,
            cancellationToken);
    }
}