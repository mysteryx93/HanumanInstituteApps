using HanumanInstitute.Services;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace HanumanInstitute.Downloads;

/// <inheritdoc cref="IDownloadManager"/>
public sealed class DownloadManager : IDownloadManager, IDisposable
{
    private readonly IDownloadTaskFactory _taskFactory;
    private readonly IOptions<DownloadOptions> _options;
    private readonly IYouTubeDownloader _youTube;
    private readonly IYouTubeStreamSelector _streamSelector;
    public const int MaxConcurrentDownloads = 50;

    public DownloadManager(IDownloadTaskFactory taskFactory, IYouTubeDownloader youTube, IYouTubeStreamSelector streamSelector,
        IOptions<DownloadOptions> options)
    {
        _taskFactory = taskFactory.CheckNotNull(nameof(taskFactory));
        _youTube = youTube.CheckNotNull(nameof(youTube));
        _streamSelector = streamSelector.CheckNotNull(nameof(_streamSelector));
        _options = options.CheckNotNull(nameof(options));

        _pool = new SemaphoreSlimDynamic(options.Value.ConcurrentDownloads, MaxConcurrentDownloads);
    }

    /// <inheritdoc />
    public event DownloadTaskEventHandler? DownloadAdded;

    private readonly SemaphoreSlimDynamic _pool;

    /// <inheritdoc />
    public async Task<Video> QueryVideoAsync(string url, CancellationToken cancellationToken = default) =>
        await QueryVideoAsync(new Uri(url), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Video> QueryVideoAsync(Uri url, CancellationToken cancellationToken = default)
    {
        var id = ParseVideoId(url.CheckNotNull(nameof(url)));
        return await _youTube.QueryVideoAsync(id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StreamManifest> QueryStreamInfoAsync(string url, CancellationToken cancellationToken = default) =>
        await QueryStreamInfoAsync(new Uri(url), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<StreamManifest> QueryStreamInfoAsync(Uri url, CancellationToken cancellationToken = default)
    {
        var id = ParseVideoId(url.CheckNotNull(nameof(url)));
        return await _youTube.QueryStreamInfoAsync(id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public StreamQueryInfo SelectStreams(StreamManifest streams, bool downloadVideo = true, bool downloadAudio = true,
        DownloadOptions? options = null)
    {
        return _streamSelector.SelectStreams(streams, downloadVideo, downloadAudio, options);
    }

    // /// <inheritdoc />
    // public Task<DownloadStatus> DownloadAsync(string downloadUrl, string destination, bool downloadVideo = true,
    //     bool downloadAudio = true, DownloadOptions? options = null, DownloadTaskEventHandler? taskCreatedCallback = null) =>
    //     DownloadAsync(new Uri(downloadUrl), destination, downloadVideo, downloadAudio, options, taskCreatedCallback);
    //
    // /// <inheritdoc />
    // public async Task<DownloadStatus> DownloadAsync(Uri downloadUrl, string destination, bool downloadVideo = true,
    //     bool downloadAudio = true, DownloadOptions? options = null, DownloadTaskEventHandler? taskCreatedCallback = null)
    // {
    //     var vInfo = await QueryStreamInfoAsync(downloadUrl).ConfigureAwait(false);
    //     var streams = SelectStreams(vInfo, downloadVideo, downloadAudio, options);
    //     return await DownloadAsync(streams, destination, taskCreatedCallback).ConfigureAwait(false);
    // }

    /// <inheritdoc />
    public async Task<DownloadStatus> DownloadAsync(StreamQueryInfo streamQuery, string destination,
        DownloadTaskEventHandler? taskCreatedCallback = null)
    {
        destination.CheckNotNullOrEmpty(nameof(destination));

        var task = _taskFactory.Create(streamQuery, destination);

        // Notify UI of new download to show window.
        var e = new DownloadTaskEventArgs(task);
        taskCreatedCallback?.Invoke(this, e);
        DownloadAdded?.Invoke(this, e);

        // Adjust pool size if ConcurrentDownloads settings changed.
        _pool.ChangeCapacity(_options.Value.ConcurrentDownloads);

        // Wait for pool to be ready.
        await _pool.WaitAsync().ConfigureAwait(false);

        // Download the file(s).
        try
        {
            await task.DownloadAsync().ConfigureAwait(false);
        }
        finally
        {
            // Release the pool and allow next download to start.
            _pool.TryRelease();
            _pool.ChangeCapacity(_options.Value.ConcurrentDownloads);
        }

        return task.Status;
    }

    /// <summary>
    /// Parses the VideoId and throws a UriFormatException if the Uri is invalid.
    /// </summary>
    /// <param name="url">The Url to download from.</param>
    /// <returns>The parsed YouTube VideoId.</returns>
    /// <exception cref="UriFormatException">Uri does not contain a valid YouTube video ID.</exception>
    private static VideoId ParseVideoId(Uri url)
    {
        url.CheckNotNull(nameof(url));

        try
        {
            var result = VideoId.Parse(url.AbsoluteUri);
            if (string.IsNullOrEmpty(result.Value))
            {
                throw new UriFormatException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidYouTubeId, url.AbsoluteUri));
            }
            return result;
        }
        catch (ArgumentException)
        {
            throw new UriFormatException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidYouTubeId, url.AbsoluteUri));
        }
    }

    private bool _disposedValue;
    /// <inheritdoc cref="IDisposable"/>
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _pool.Dispose();
            }
            _disposedValue = true;
        }
    }

    /// <inheritdoc cref="IDisposable"/>
    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this);
    }
}
