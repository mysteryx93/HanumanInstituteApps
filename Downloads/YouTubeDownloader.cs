using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads;

/// <inheritdoc />
public sealed class YouTubeDownloader : IYouTubeDownloader
{
    private readonly YoutubeClient _youTube;

    public YouTubeDownloader(YoutubeClient youTube)
    {
        _youTube = youTube;
    }

    /// <inheritdoc />
    public async Task<Video> QueryVideoAsync(VideoId videoId, CancellationToken cancellationToken = default)
    {
        videoId.Value.CheckNotNullOrEmpty(nameof(videoId));
        return await _youTube.Videos.GetAsync(videoId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StreamManifest> QueryStreamInfoAsync(VideoId videoId, CancellationToken cancellationToken = default)
    {
        videoId.Value.CheckNotNullOrEmpty(nameof(videoId));
        return await _youTube.Videos.Streams.GetManifestAsync(videoId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DownloadAsync(IStreamInfo streamInfo, string filePath, Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        await _youTube.Videos.Streams.DownloadAsync(streamInfo,
            filePath,
            progressCallback != null ? new Progress<double>(progressCallback) : null,
            cancellationToken).ConfigureAwait(false);
    }
}
