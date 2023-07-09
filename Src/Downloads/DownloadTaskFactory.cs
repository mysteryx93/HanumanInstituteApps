using HanumanInstitute.BassAudio;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.Services;

namespace HanumanInstitute.Downloads;

/// <inheritdoc />
public sealed class DownloadTaskFactory : IDownloadTaskFactory
{
    private readonly IYouTubeDownloader _youTube;
    private readonly IFileSystemService _fileSystem;
    private readonly IMediaMuxer _mediaMuxer;
    private readonly IAudioEncoder _audioEncoder;

    public DownloadTaskFactory(IYouTubeDownloader youTube, IFileSystemService fileSystem, IMediaMuxer mediaMuxer,
        IAudioEncoder audioEncoder)
    {
        _youTube = youTube;
        _fileSystem = fileSystem;
        _mediaMuxer = mediaMuxer;
        _audioEncoder = audioEncoder;
        _mediaMuxer.Owner = this;
    }

    /// <inheritdoc />
    public IDownloadTask Create(StreamQueryInfo streamQuery, string destination) =>
        new DownloadTask(_youTube, _fileSystem, _mediaMuxer, _audioEncoder, streamQuery, destination);
}
