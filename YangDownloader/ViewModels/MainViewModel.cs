using System.Net.Http;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Downloads;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.YangDownloader.Business;
using ReactiveUI;
using YoutubeExplode.Videos.Streams;
using SplatContainerExtensions = HanumanInstitute.Downloads.SplatContainerExtensions;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class MainViewModel : MainViewModelBase<AppSettingsData>, ICloseable
{
    private readonly IDownloadManager _downloadManager;
    private readonly IYouTubeStreamSelector _streamSelector;
    private readonly IDialogService _dialogService;
    private readonly IFileSystemService _fileSystem;
    private readonly IMediaInfoReader? _ffmpegInfo;
    private readonly IClipboard _clipboard;

    public MainViewModel(ISettingsProvider<AppSettingsData> settings, IAppUpdateService appUpdateService, IDownloadManager downloadManager,
        IYouTubeStreamSelector streamSelector, IDialogService dialogService, IFileSystemService fileSystem, IMediaInfoReader? ffmpegInfo,
        IClipboard clipboard, IEnvironmentService environment) :
        base(settings, appUpdateService, environment)
    {
        _downloadManager = downloadManager;
        _streamSelector = streamSelector;
        _dialogService = dialogService;
        _fileSystem = fileSystem;
        _ffmpegInfo = ffmpegInfo;
        _clipboard = clipboard;

        PreferredVideo = new ListItemCollectionView<StreamContainerOption>()
        {
            { StreamContainerOption.Best },
            { StreamContainerOption.MP4 },
            { StreamContainerOption.WebM },
            { StreamContainerOption.None }
        };
        PreferredVideo.MoveCurrentToFirst();

        PreferredAudio = new ListItemCollectionView<StreamContainerOption>()
        {
            { StreamContainerOption.Best },
            { StreamContainerOption.MP4 },
            { StreamContainerOption.WebM },
            { StreamContainerOption.None }
        };
        PreferredAudio.MoveCurrentToFirst();

        var quality = new List<ListItem<int>> { { 0, Resources.Max } };
        foreach (var res in new[] { 4320, 3072, 2160, 1440, 1080, 720, 480, 360, 240, 144 })
        {
            quality.Add(res, "{0}p".FormatInvariant(res));
        }
        MaxQuality = new ListItemCollectionView<int>(quality);

        _downloadManager.DownloadAdded += DownloadManager_DownloadAdded;

        _hasDownloads = Downloads
            .ToObservableChangeSet()
            .ToCollection()
            .Select(x => x.Any())
            .ToProperty(this, x => x.HasDownloads);
    }

    /// <inheritdoc />
    public event EventHandler? RequestClose;

    public override async void OnLoaded()
    {
        base.OnLoaded();
        await CheckFFmpegAsync().ConfigureAwait(false);
    }

    protected override void ConvertFromSettings()
    {
        PreferredVideo.SelectedValue = Settings.PreferredVideo;
        PreferredAudio.SelectedValue = Settings.PreferredAudio;
        MaxQuality.SelectedValue = Settings.MaxQuality;
    }

    protected override void ConvertToSettings()
    {
        Settings.PreferredVideo = PreferredVideo.SelectedValue;
        Settings.PreferredAudio = PreferredAudio.SelectedValue;
        Settings.MaxQuality = MaxQuality.SelectedValue;
    }

    /// <summary>
    /// Gets whether to show the downloads list.
    /// </summary>
    public bool HasDownloads => _hasDownloads.Value;
    private readonly ObservableAsPropertyHelper<bool> _hasDownloads;

    /// <summary>
    /// Gets or sets the type of video stream to download.
    /// </summary>
    public ListItemCollectionView<StreamContainerOption> PreferredVideo { get; }

    /// <summary>
    /// Gets or sets the type of audio stream to download.
    /// </summary>
    public ListItemCollectionView<StreamContainerOption> PreferredAudio { get; }

    /// <summary>
    /// Gets or sets the maximum quality of the stream to download.
    /// </summary>
    public ListItemCollectionView<int> MaxQuality { get; }

    /// <summary>
    /// Gets or sets the URL to download.
    /// </summary>
    [Reactive]
    public string DownloadUrl { get; set; } = string.Empty;
    private Uri? _downloadUri;

    /// <summary>
    /// Gets or sets whether to download the video stream.
    /// </summary>
    [Reactive]
    public bool DownloadVideo { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to download the audio stream.
    /// </summary>
    [Reactive]
    public bool DownloadAudio { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to display download info.
    /// </summary>
    [Reactive]
    public bool DisplayDownloadInfo { get; protected set; }

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    [Reactive]
    public string ErrorMessage { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the video.
    /// </summary>
    [Reactive]
    public string VideoTitle { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the information of the selected video stream.
    /// </summary>
    [Reactive]
    public string VideoStreamInfo { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the information of the selected audio stream.
    /// </summary>
    [Reactive]
    public string AudioStreamInfo { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the list of downloads.
    /// </summary>
    public ObservableCollection<DownloadItem> Downloads { get; } = new();

    /// <summary>
    /// Gets or sets whether the selection is valid to start downloading. 
    /// </summary>
    [Reactive]
    protected bool IsDownloadValid { get; private set; }

    /// <summary>
    /// Shows the encode settings window.
    /// </summary>
    public RxCommandUnit ShowEncodeSettings => _showEncodeSettings ??= ReactiveCommand.CreateFromTask(ShowEncodeSettingsImpl);
    private RxCommandUnit? _showEncodeSettings;
    private async Task ShowEncodeSettingsImpl()
    {
        await _dialogService.ShowEncodeSettingsAsync(this, Settings.EncodeSettings);
    }

    /// <summary>
    /// When activating the window, auto-paste URL and query.
    /// </summary>
    public RxCommandUnit ViewActivated => _viewActivated ??= ReactiveCommand.CreateFromTask(ViewActivatedImplAsync);
    private RxCommandUnit? _viewActivated;
    private async Task ViewActivatedImplAsync()
    {
        if (string.IsNullOrWhiteSpace(DownloadUrl) || !DisplayDownloadInfo)
        {
            var text = await _clipboard.GetTextAsync().ConfigureAwait(true);
            if (text?.Length < 128 && IsValidUrl(text))
            {
                DownloadUrl = text;
                Query.Execute().Subscribe();
            }
        }
    }
    
    private bool IsValidUrl(string url)
    {
        const string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        var rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return rgx.IsMatch(url);
    }

    /// <summary>
    /// Queries download information and selects preferred streams.
    /// </summary>
    public RxCommandUnit Query => _query ??= ReactiveCommand.CreateFromTask(QueryImpl,
        this.WhenAnyValue(x => x.DownloadUrl, url => !string.IsNullOrWhiteSpace(url)));
    private RxCommandUnit? _query;
    private async Task QueryImpl()
    {
        // Reset.
        IsDownloadValid = false;
        DisplayDownloadInfo = true;
        ErrorMessage = string.Empty;
        VideoTitle = Resources.Querying;
        VideoStreamInfo = string.Empty;
        AudioStreamInfo = string.Empty;

        // Validate.
        if (PreferredVideo.CurrentItem?.Value == StreamContainerOption.None &&
            PreferredAudio.CurrentItem?.Value == StreamContainerOption.None)
        {
            SetError(Resources.SetPreferredFormats);
            return;
        }

        try
        {
            _downloadUri = new Uri(DownloadUrl);
        }
        catch (UriFormatException)
        {
            SetError(Resources.InvalidUrl);
            return;
        }

        // Query.
        StreamManifest? streams = null;
        var success = await TryQueryAsync(async () =>
        {
            // Keep on main thread so we can update UI.
            // Run concurrently and display title as soon as available.
            var t1 = _downloadManager.QueryVideoAsync(_downloadUri).ConfigureAwait(true);
            var t2 = _downloadManager.QueryStreamInfoAsync(_downloadUri).ConfigureAwait(true);
            var info = await t1;
            VideoTitle = info.Title;
            streams = await t2;
        }).ConfigureAwait(true);
        if (!success)
        {
            VideoTitle = string.Empty;
            return;
        }

        // Display.
        if (streams?.Streams.Any() == true)
        {
            var query = _streamSelector.SelectStreams(streams, true, true, GetDownloadOptions());
            if (query.Video != null || query.Audio != null)
            {
                IsDownloadValid = true;

                if (query.Video is MuxedStreamInfo)
                {
                    VideoStreamInfo = "{0} - {1} ({2:N1}mb) ({3})".FormatInvariant(
                        query.Video.VideoCodec,
                        query.Video.VideoQuality.Label,
                        query.Video.Size.MegaBytes,
                        Resources.MuxedAudio);
                    AudioStreamInfo = "{0}".FormatInvariant(query.Audio?.AudioCodec);
                }
                else
                {
                    if (query.Video != null)
                    {
                        VideoStreamInfo = "{0} - {1} ({2:N1}mb)".FormatInvariant(
                            query.Video.VideoCodec,
                            query.Video.VideoQuality.Label,
                            query.Video.Size.MegaBytes);
                    }
                    if (query.Audio != null)
                    {
                        AudioStreamInfo = "{0} - {1:N0}kbps ({2:N1}mb)".FormatInvariant(
                            query.Audio.AudioCodec,
                            query.Audio.Bitrate.KiloBitsPerSecond,
                            query.Audio.Size.MegaBytes);
                    }
                }
            }
            else
            {
                SetError(Resources.NoStreamInfo);
            }
        }
        else
        {
            SetError(Resources.InvalidUrl);
        }
    }

    /// <summary>
    /// Starts a download task.
    /// </summary>
    public RxCommandUnit Download => _download ??= ReactiveCommand.CreateFromTask(DownloadImpl,
        this.WhenAnyValue(x => x.IsDownloadValid));
    private RxCommandUnit? _download;
    private async Task DownloadImpl()
    {
        if (string.IsNullOrWhiteSpace(Settings.DestinationFolder))
        {
            ErrorMessage = Resources.DestinationMissing;
            return;
        }
        else if (!_fileSystem.Directory.Exists(Settings.DestinationFolder))
        {
            ErrorMessage = Resources.DestinationDoesNotExist;
            return;
        }
        else if (!(AudioStreamInfo.HasValue() && DownloadAudio) && !(VideoStreamInfo.HasValue() && DownloadVideo))
        {
            ErrorMessage = Resources.NoStreamSelected;
            return;
        }

        IsDownloadValid = false;

        StreamManifest? streams = null;
        var success = await TryQueryAsync(async () =>
        {
            streams = await _downloadManager.QueryStreamInfoAsync(_downloadUri!).ConfigureAwait(true);
        }).ConfigureAwait(false);
        if (!success)
        {
            return;
        }

        if (streams?.Streams.Any() == true)
        {
            var query = _downloadManager.SelectStreams(streams, DownloadVideo, DownloadAudio, GetDownloadOptions());
            var fileName = string.IsNullOrWhiteSpace(VideoTitle) ? Resources.DefaultFileName : _fileSystem.SanitizeFileName(VideoTitle);

            // Avoid conflicting file names by adding (2) after file name.
            var destination = _fileSystem.Path.Combine(Settings.DestinationFolder, fileName);
            var suffix = "." + query.FileExtension;
            var i = 1;
            while (_fileSystem.File.Exists(destination + suffix))
            {
                i++;
                suffix = $" ({i}).{query.FileExtension}";
            }
            destination += suffix;

            var _ = _downloadManager.DownloadAsync(query, destination).ContinueWith(async x =>
            {
                if (x.Exception != null)
                {
                    await _dialogService.ShowMessageBoxAsync(this, x.Exception.InnerException!.ToString(), "Download error");
                }
            });
        }
    }

    /// <summary>
    /// Tries to query data and displays an error message if it fails.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>Whether the action succeeded.</returns>
    private async Task<bool> TryQueryAsync(Func<Task> action)
    {
        try
        {
            await action.Invoke().ConfigureAwait(true);
            return true;
        }
        catch (TaskCanceledException)
        {
            SetError(string.Empty);
        }
        catch (HttpRequestException)
        {
            SetError(Resources.ConnectionFailed);
        }
        catch (UriFormatException)
        {
            SetError(Resources.InvalidUrl);
        }
        catch (Exception e)
        {
            SetError(e.Message);
        }
        return false;
    }

    private void SetError(string text)
    {
        ErrorMessage = text;
        DisplayDownloadInfo = false;
    }

    private DownloadOptions GetDownloadOptions() => new()
    {
        PreferredVideo = PreferredVideo.CurrentItem!.Value,
        PreferredAudio = PreferredAudio.CurrentItem!.Value,
        MaxQuality = MaxQuality.CurrentItem!.Value,
        ConcurrentDownloads = 2,
        EncodeAudio = Settings.EncodeAudio ? Settings.EncodeSettings : null
    };

    private void DownloadManager_DownloadAdded(object sender, DownloadTaskEventArgs e) => Dispatcher.UIThread.Post(() =>
        Downloads.Add(new DownloadItem(e.Download, VideoTitle))
    );

    /// <inheritdoc />
    protected override Task ShowAboutImplAsync() => _dialogService.ShowAboutAsync(this);

    /// <inheritdoc />
    protected override Task ShowSettingsImplAsync() => _dialogService.ShowSettingsAsync(this, _settings.Value);

    private async Task CheckFFmpegAsync()
    {
        if (_ffmpegInfo == null) { return; }

        var found = false;
        try
        {
            var version = _ffmpegInfo.GetVersion();
            found = version.HasValue();
        }
        catch { }

        if (!found)
        {
            var msg = "FFmpeg not found! Please install FFmpeg on your system.";
            await _dialogService.ShowMessageBoxAsync(this, msg, "Error", MessageBoxButton.Ok).ConfigureAwait(true);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
