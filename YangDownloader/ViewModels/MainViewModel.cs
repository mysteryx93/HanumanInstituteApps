using System.Net.Http;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.BassAudio;
using HanumanInstitute.Converter432hz.Business;
using HanumanInstitute.Downloads;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.YangDownloader.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly IDownloadManager _downloadManager;
        private readonly IYouTubeStreamSelector _streamSelector;
        private readonly IDialogService _dialogService;
        private readonly IFileSystemService _fileSystem;

        public MainViewModel(IDownloadManager downloadManager, IYouTubeStreamSelector streamSelector, IDialogService dialogService,
            IFileSystemService fileSystem)
        {
            _downloadManager = downloadManager;
            _streamSelector = streamSelector;
            _dialogService = dialogService;
            _fileSystem = fileSystem;

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
        public string DownloadUrl { get; set; } = "https://www.youtube.com/watch?v=4OqXWzekVw4"; // string.Empty;
        private Uri? _downloadUri;

        /// <summary>
        /// Gets or sets the folder where to save downloaded files.
        /// </summary>
        [Reactive]
        public string DestinationFolder { get; set; } = string.Empty;

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
        public string Message { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the video.
        /// </summary>
        [Reactive]
        public string VideoTitle { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the information of the selected video stream.
        /// </summary>
        [Reactive]
        public string VideoStreamInfo { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the information of the selected audio stream.
        /// </summary>
        [Reactive]
        public string AudioStreamInfo { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the list of downloads.
        /// </summary>
        public ObservableCollection<DownloadItem> Downloads { get; } = new ObservableCollection<DownloadItem>();

        /// <summary>
        /// Gets or sets whether to re-encode the audio stream.
        /// </summary>
        [Reactive]
        public bool EncodeAudio { get; set; }

        /// <summary>
        /// Gets or sets audio encode settings.
        /// </summary>
        [Reactive]
        public EncodeSettings EncodeAudioSettings { get; set; } = new EncodeSettings();

        /// <summary>
        /// Gets or sets whether the selection is valid to start downloading. 
        /// </summary>
        [Reactive]
        protected bool IsDownloadValid { get; private set; }

        /// <summary>
        /// Shows the open folder dialog to select destination. 
        /// </summary>
        public ICommand BrowseDestination => _browseDestination ??= ReactiveCommand.CreateFromTask(BrowseDestinationImpl);
        private ICommand? _browseDestination;
        private async Task BrowseDestinationImpl()
        {
            var options = new OpenFolderDialogSettings()
            {
                Title = "Select destination folder",
                InitialDirectory = DestinationFolder
            };
            var result = await _dialogService.ShowOpenFolderDialogAsync(this, options).ConfigureAwait(false);
            if (result != null)
            {
                DestinationFolder = result;
                Message = "";
            }
        }

        /// <summary>
        /// Shows the encode settings window.
        /// </summary>
        public ICommand ShowEncodeSettings => _showEncodeSettings ??= ReactiveCommand.CreateFromTask(ShowEncodeSettingsImpl);
        private ICommand? _showEncodeSettings;
        private async Task ShowEncodeSettingsImpl()
        {
            await _dialogService.ShowEncodeSettingsAsync(this, EncodeAudioSettings);
        }

        /// <summary>
        /// Queries download information and selects preferred streams.
        /// </summary>
        public ICommand Query => _query ??= ReactiveCommand.CreateFromTask(QueryImpl,
            this.WhenAnyValue(x => x.DownloadUrl, url => !string.IsNullOrWhiteSpace(url)));
        private ICommand? _query;
        private async Task QueryImpl()
        {
            // Reset.
            IsDownloadValid = false;
            DisplayDownloadInfo = true;
            Message = string.Empty;
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
        public ICommand Download => _download ??= ReactiveCommand.CreateFromTask(DownloadImpl,
            this.WhenAnyValue(x => x.IsDownloadValid));
        private ICommand? _download;
        private async Task DownloadImpl()
        {
            if (string.IsNullOrWhiteSpace(DestinationFolder))
            {
                Message = Resources.DestinationMissing;
                return;
            }
            else if (!_fileSystem.Directory.Exists(DestinationFolder))
            {
                Message = Resources.DestinationDoesNotExist;
                return;
            }
            else if (!(AudioStreamInfo.HasValue() && DownloadAudio) && !(VideoStreamInfo.HasValue() && DownloadVideo))
            {
                Message = Resources.NoStreamSelected;
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
                var destination = _fileSystem.Path.Combine(DestinationFolder, $"{fileName}.{query.FileExtension}");

                await _downloadManager.DownloadAsync(query, destination).ConfigureAwait(false);
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
            return false;
        }
        
        private void SetError(string text)
        {
            Message = text;
            DisplayDownloadInfo = false;
        }

        private DownloadOptions GetDownloadOptions() =>
            new DownloadOptions()
            {
                PreferredVideo = PreferredVideo.CurrentItem!.Value,
                PreferredAudio = PreferredAudio.CurrentItem!.Value,
                MaxQuality = MaxQuality.CurrentItem!.Value,
                ConcurrentDownloads = 2
            };

        private void DownloadManager_DownloadAdded(object sender, DownloadTaskEventArgs e) =>
            Downloads.Add(new DownloadItem(e.Download, VideoTitle));
    }
}
