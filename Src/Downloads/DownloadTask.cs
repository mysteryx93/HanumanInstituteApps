using System.IO;
using System.Net.Http;
using HanumanInstitute.BassAudio;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.Services;
using ManagedBass;
using YoutubeExplode.Videos.Streams;

// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.Downloads;

/// <inheritdoc cref="IDownloadTask" />
public sealed class DownloadTask : IDownloadTask
{
    private readonly IYouTubeDownloader _youTube;
    private readonly IFileSystemService _fileSystem;
    private readonly IMediaMuxer _mediaMuxer;
    private readonly IAudioEncoder _audioEncoder;

    public DownloadTask(IYouTubeDownloader youTube, IFileSystemService fileSystem, IMediaMuxer mediaMuxer, IAudioEncoder audioEncoder, 
        StreamQueryInfo streamsQuery, string destination)
    {
        Query = streamsQuery.CheckNotNull(nameof(streamsQuery));
        if (Query.OutputVideo == null && Query.OutputAudio == null) { throw new ArgumentException(Resources.RequestHasNoStream); }

        _youTube = youTube;
        _fileSystem = fileSystem;
        _mediaMuxer = mediaMuxer;
        _audioEncoder = audioEncoder;

        Destination = destination.CheckNotNullOrEmpty(nameof(destination));
    }

    /// <inheritdoc />
    public string Destination { get; set; }
    /// <inheritdoc />
    public StreamQueryInfo Query { get; private set; }
    /// <inheritdoc />
    public IList<DownloadTaskFile> Files { get; } = new List<DownloadTaskFile>();
    private DownloadStatus _status = DownloadStatus.Waiting;
    private bool _isCalled;
    private readonly CancellationTokenSource _cancelToken = new();

    /// <inheritdoc />
    public event MuxeTaskEventHandler? Muxing;
    /// <inheritdoc />
    public event DownloadTaskEventHandler? ProgressUpdated;
    /// <inheritdoc />
    public double ProgressValue { get; private set; }
    /// <inheritdoc />
    public string ProgressText { get; set; } = string.Empty;

    /// <inheritdoc />
    public async Task DownloadAsync()
    {
        // Ensure this method is called only once.
        if (_isCalled) { throw new InvalidOperationException(Resources.DownloadTaskCallOnce); }
        _isCalled = true;

        // The task might have been cancelled while waiting in queue.
        if (IsCancelled) { return; }

        // Start download.
        Status = DownloadStatus.Initializing;
        _fileSystem.EnsureDirectoryExists(Destination);
        await _fileSystem.File.WriteAllTextAsync(Destination, string.Empty).ConfigureAwait(false);

        // Make sure we could retrieve download streams.
        if (Query.Video == null && Query.Audio == null)
        {
            Status = DownloadStatus.Failed;
            return;
        }

        Task? taskVideo = null;
        Task? taskAudio = null;

        // Add the best video stream.
        if (Query.Video != null)
        {
            var fileVideo = new DownloadTaskFile(true, Query.OutputAudio != null && Query.Audio == null, new Uri(Query.Video.Url),
                Destination + ".tempvideo", Query.Video, Query.Video.Size.Bytes);
            Files.Add(fileVideo);
            taskVideo = DownloadFileAsync(fileVideo);
        }

        // Add the best audio stream.
        if (Query.Audio != null)
        {
            var fileAudio = new DownloadTaskFile(Query.OutputVideo != null && Query.Video == null, true, new Uri(Query.Audio.Url),
                Destination + ".tempaudio", Query.Audio, Query.Audio.Size.Bytes);
            Files.Add(fileAudio);
            taskAudio = DownloadFileAsync(fileAudio);
            if (Query.EncodeAudio != null)
            {
                taskAudio = await taskAudio.ContinueWith(_ =>
                    EncodeAudioAsync(fileAudio, Query.EncodeAudio));
            }
        }

        if (!IsCancelled)
        {
            await (taskAudio ?? Task.CompletedTask).ConfigureAwait(false);
            await (taskVideo ?? Task.CompletedTask).ConfigureAwait(false);

            if (VerifyFiles())
            {
                await DownloadCompletedAsync().ConfigureAwait(false);
            }
            else
            {
                Status = DownloadStatus.Failed;
            }
        }

        await Task.Delay(100).ConfigureAwait(false);
        DeleteTempFiles();
    }

    /// <summary>
    /// Returns whether all files are finished downloading.
    /// </summary>
    private bool VerifyFiles()
    {
        if (IsCancelled || Files.Count <= 0) { return false; }

        foreach (var item in Files)
        {
            if (!FileHasMinSize(item.Destination, item.Length) ||
                (item.DestinationEncoded.HasValue() && !FileHasMinSize(item.DestinationEncoded)))
            {
                return false;
            }
        }
        return true;

        bool FileHasMinSize(string file, long minSize = 1) =>
            _fileSystem.File.Exists(file) && _fileSystem.FileInfo.New(file).Length >= minSize;
    }

    /// <summary>
    /// Downloads a file stream. There can be multiple streams per download.
    /// </summary>
    /// <param name="fileInfo">Information about the specific file stream to download.</param>
    private async Task DownloadFileAsync(DownloadTaskFile fileInfo)
    {
        Status = DownloadStatus.Downloading;

        try
        {
            await _youTube.DownloadAsync(
                (IStreamInfo)fileInfo.Stream, fileInfo.Destination, ProgressHandler, _cancelToken.Token).ConfigureAwait(false);

            void ProgressHandler(double percent)
            {
                fileInfo.Downloaded = (long)(fileInfo.Length * percent);
                UpdateProgress();
            }
        }
        catch (TaskCanceledException) { Status = DownloadStatus.Canceled; }
        catch (HttpRequestException) { Status = DownloadStatus.Failed; }
        catch (IOException) { Status = DownloadStatus.Failed; }
        catch (Exception)
        {
            Status = DownloadStatus.Failed;
            throw;
        }
    }

    /// <summary>
    /// Re-encodes the audio based on Query.EncodeAudio.
    /// </summary>
    /// <param name="fileInfo">Information about the file stream downloaded.</param>
    /// <param name="encodeSettings">The encoding settings.</param>
    private async Task EncodeAudioAsync(DownloadTaskFile fileInfo, EncodeSettings encodeSettings)
    {
        // We must first muxe the audio into a container so that BASS can read it.
        var audioInfo = (IAudioStreamInfo)fileInfo.Stream;
        var dest = fileInfo.Destination + (audioInfo.Container == Container.WebM ? ".opus" : ".m4a");
        fileInfo.DestinationMuxed = dest;
        var muxeStatus = _mediaMuxer.Muxe(null, fileInfo.Destination, dest, 
            new ProcessOptionsEncoder(ProcessDisplayMode.None));
        if (muxeStatus != CompletionStatus.Success)
        {
            Status = DownloadStatus.Failed;
            return;
        }
        
        // Encode the audio.
        dest = fileInfo.Destination + ".encode";
        fileInfo.DestinationEncoded = dest;
        var item = new ProcessingItem(fileInfo.DestinationMuxed, fileInfo.Destination) { Destination = dest };
        try
        {
            await _audioEncoder.StartAsync(item, encodeSettings, _cancelToken.Token);
        }
        catch (FileNotFoundException) { Status = DownloadStatus.Failed; }
        catch (BassException) { Status = DownloadStatus.Failed; }
        catch (Exception)
        {
            Status = DownloadStatus.Failed;
            throw;
        }
    }

    /// <summary>
    /// Occurs when a file download is completed.
    /// </summary>
    private async Task DownloadCompletedAsync()
    {
        Status = DownloadStatus.Finalizing;
        _fileSystem.DeleteFileSilent(Destination);

        var videoFile = Files.FirstOrDefault(f => f.HasVideo);
        var audioFile = Files.FirstOrDefault(f => f.HasAudio);

        // Handle already-muxed files containing both audio and video.
        if (videoFile?.HasAudio == true)
        {
            if (Query.DownloadVideo && Query.DownloadAudio)
            {
                // Move file, no muxing required.
                try
                {
                    _fileSystem.File.Move(videoFile.Destination, Destination);
                    Status = DownloadStatus.Success;
                }
                catch (IOException) { Status = DownloadStatus.Failed; }
                catch (UnauthorizedAccessException) { Status = DownloadStatus.Failed; }
                catch (Exception)
                {
                    Status = DownloadStatus.Failed;
                    throw;
                }
            }
            else
            {
                // Extract one stream.
                await MuxeStreams(Query.DownloadVideo ? videoFile.DestinationEncoded ?? videoFile.Destination : null,
                        Query.DownloadAudio ? videoFile.DestinationEncoded ?? videoFile.Destination : null)
                    .ConfigureAwait(false);
            }
        }
        else
        {
            // Muxe normal streams.
            await MuxeStreams(videoFile?.DestinationEncoded ?? videoFile?.Destination, 
                audioFile?.DestinationEncoded ?? audioFile?.Destination).ConfigureAwait(false);
        }
    }

    private async Task MuxeStreams(string? videoFile, string? audioFile)
    {
        // Allow custom muxing.
        if (Muxing != null)
        {
            try
            {
                await Task.Run(() => Muxing?.Invoke(this, new MuxeTaskEventArgs(this, videoFile, audioFile))).ConfigureAwait(false);
            }
            catch
            {
                Status = DownloadStatus.Failed;
                throw;
            }
        }

        if (IsCancelled)
        {
            DeleteTempFiles();
            return;
        }

        var muxeSuccess = _fileSystem.File.Exists(Destination);

        // If not done through event, do standard muxing.
        if (!muxeSuccess)
        {
            var taskResult = await Task.Run(() => _mediaMuxer.Muxe(videoFile, audioFile, Destination)).ConfigureAwait(false);
            muxeSuccess = taskResult == CompletionStatus.Success;
        }

        if (muxeSuccess)
        {
            muxeSuccess = _fileSystem.File.Exists(Destination);
        }

        Status = muxeSuccess ? DownloadStatus.Success : DownloadStatus.Failed;
    }

    /// <summary>
    /// Delete partially-downloaded files.
    /// </summary>
    private void DeleteTempFiles()
    {
        if (_fileSystem.File.Exists(Destination) && _fileSystem.FileInfo.New(Destination).Length == 0)
        {
            _fileSystem.File.Delete(Destination);
        }
        foreach (var item in Files)
        {
            _fileSystem.DeleteFileSilent(item.Destination);
            if (item.DestinationMuxed.HasValue())
            {
                _fileSystem.DeleteFileSilent(item.DestinationMuxed);
            }
            if (item.DestinationEncoded.HasValue())
            {
                _fileSystem.DeleteFileSilent(item.DestinationEncoded);
            }
        }
    }

    /// <inheritdoc />
    public DownloadStatus Status
    {
        get => _status;
        private set
        {
            if (_status == value) { return; }

            // Updates the status information.
            _status = value;
            ProgressText = value switch
            {
                DownloadStatus.Waiting => Resources.StatusWaiting,
                DownloadStatus.Initializing => Resources.StatusInitializing,
                DownloadStatus.Downloading => Resources.StatusInitializing,
                DownloadStatus.Success => Resources.StatusDone,
                DownloadStatus.Finalizing => Resources.StatusFinalizing,
                DownloadStatus.Canceled => Resources.StatusCanceled,
                DownloadStatus.Failed => Resources.StatusFailed,
                _ => string.Empty
            };

            if (_status is DownloadStatus.Canceled or DownloadStatus.Failed)
            {
                try
                {
                    _cancelToken.Cancel();
                }
                catch (ObjectDisposedException) { } // In case task is already done.
            }

            ProgressUpdated?.Invoke(this, new DownloadTaskEventArgs(this));
        }
    }

    /// <summary>
    /// Updates the status text with progress percentage. Only call this method if status is Downloading.
    /// </summary>
    private void UpdateProgress()
    {
        if (Status != DownloadStatus.Downloading) { return; }

        long totalBytes = 0;
        long downloaded = 0;
        var bytesTotalLoaded = true;
        foreach (var item in Files)
        {
            if (item.Length > 0)
            {
                totalBytes += item.Length;
            }
            else
            {
                bytesTotalLoaded = false;
            }

            downloaded += item.Downloaded;
        }
        if (bytesTotalLoaded)
        {
            ProgressValue = (double)downloaded / totalBytes;
            ProgressText = ProgressValue.ToString("p1", CultureInfo.CurrentCulture);
        }
        try
        {
            ProgressUpdated?.Invoke(this, new DownloadTaskEventArgs(this));
        }
        catch
        {
            Status = DownloadStatus.Failed;
            throw;
        }
    }

    /// <inheritdoc />
    public void Cancel()
    {
        if (Status != DownloadStatus.Success && Status != DownloadStatus.Failed)
        {
            Status = DownloadStatus.Canceled;
        }
    }

    /// <inheritdoc />
    public void Fail()
    {
        if (Status != DownloadStatus.Success)
        {
            Status = DownloadStatus.Failed;
        }
    }

    /// <summary>
    /// Returns whether the download was canceled or failed.
    /// </summary>
    private bool IsCancelled => (Status == DownloadStatus.Canceled || Status == DownloadStatus.Failed);

    private bool _disposedValue;
    /// <inheritdoc cref="IDisposable"/>
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cancelToken.Dispose();
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
