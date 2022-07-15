using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.Downloads;

/// <inheritdoc />
public class DownloadTaskFactory : IDownloadTaskFactory
{
    private readonly IYouTubeDownloader _youTube;
    private readonly IFileSystemService _fileSystem;
    private readonly IMediaMuxer _mediaMuxer;

    public DownloadTaskFactory(IYouTubeDownloader youTube, IFileSystemService fileSystem, IMediaMuxer mediaMuxer)
    {
        _youTube = youTube.CheckNotNull(nameof(youTube));
        _fileSystem = fileSystem.CheckNotNull(nameof(fileSystem));
        _mediaMuxer = mediaMuxer.CheckNotNull(nameof(mediaMuxer));
    }

    /// <inheritdoc />
    public IDownloadTask Create(StreamQueryInfo streamQuery, string destination) =>
        new DownloadTask(_youTube, _fileSystem, _mediaMuxer, streamQuery, destination);
}
