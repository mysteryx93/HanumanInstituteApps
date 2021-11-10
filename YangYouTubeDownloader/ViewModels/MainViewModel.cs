using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Downloads;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.YangYouTubeDownloader.Models;
using HanumanInstitute.YangYouTubeDownloader.Properties;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using PropertyChanged;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    [AddINotifyPropertyChangedInterface()]
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly IDownloadManager _downloadManager;
        private readonly IYouTubeStreamSelector _streamSelector;
        private readonly IDialogService _dialogService;

        public MainViewModel(IDownloadManager downloadManager, IYouTubeStreamSelector streamSelector, IDialogService dialogService)
        {
            _downloadManager = downloadManager.CheckNotNull(nameof(downloadManager));
            _streamSelector = streamSelector.CheckNotNull(nameof(streamSelector));
            _dialogService = dialogService;

            PreferredVideo = new ListItemCollectionView<StreamContainerOption>()
            {
                { StreamContainerOption.Best },
                { StreamContainerOption.MP4 },
                { StreamContainerOption.WebM },
                { StreamContainerOption.Tgpp },
                { StreamContainerOption.None }
            };
            PreferredVideo.MoveCurrentToFirst();

            PreferredAudio = new ListItemCollectionView<StreamContainerOption>()
            {
                { StreamContainerOption.Best },
                { StreamContainerOption.MP4 },
                { StreamContainerOption.WebM },
                { StreamContainerOption.Tgpp },
                { StreamContainerOption.None }
            };
            PreferredAudio.MoveCurrentToFirst();

            var qual = new List<ListItem<int>>
            {
                { 0, Resources.Max }
            };
            foreach (var res in new[] { 4320, 3072, 2160, 1440, 1080, 720, 480, 360, 240, 144 })
            {
                qual.Add(res, string.Format(CultureInfo.InvariantCulture, "{0}p", res));
            }
            MaxQuality = new ListItemCollectionView<int>(qual);

            _downloadManager.DownloadAdded += DownloadManager_DownloadAdded;
        }

        public string DownloadUrl { get; set; } = "https://www.youtube.com/watch?v=4OqXWzekVw4"; // string.Empty;
        public bool DownloadVideo { get; set; } = true;
        public bool DownloadAudio { get; set; } = true;

        public bool DisplayDownloadInfo { get; private set; }
        public bool IsDownloadValid { get; private set; }
        public ICollectionView<ListItem<StreamContainerOption>> PreferredVideo { get; private set; }
        public ICollectionView<ListItem<StreamContainerOption>> PreferredAudio { get; private set; }
        public ICollectionView<ListItem<int>> MaxQuality { get; private set; }
        // public ISelectableList<ListItem<StreamContainerOption>> PreferredVideo { get; private set; } = new SelectableList<ListItem<StreamContainerOption>>();
        //public ISelectableList<ListItem<StreamContainerOption>> PreferredAudio { get; private set; } = new SelectableList<ListItem<StreamContainerOption>>();
        // public ISelectableList<ListItem<int>> MaxQuality { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public string VideoTitle { get; private set; } = string.Empty;
        // public string VideoContainer { get; private set; } = string.Empty;
        public string VideoStreamInfo { get; private set; } = string.Empty;
        public string AudioStreamInfo { get; private set; } = string.Empty;
        public ObservableCollection<DownloadItem> Downloads { get; private set; } = new ObservableCollection<DownloadItem>();
        public bool IsDownloadInitializing { get; private set; }
        private Uri? _downloadUri;

        public ICommand QueryCommand => CommandHelper.InitCommand(ref _queryCommand, OnQuery, () => CanQuery);
        private RelayCommand? _queryCommand;
        private bool CanQuery => !string.IsNullOrWhiteSpace(DownloadUrl);
        private async void OnQuery()
        {
            if (!CanQuery) { return; }

            // Reset.
            IsDownloadValid = false;
            SetError();
            VideoTitle = "Querying...";
            VideoStreamInfo = string.Empty;
            AudioStreamInfo = string.Empty;

            // Validate.
            if (PreferredVideo.CurrentItem?.Value == StreamContainerOption.None && PreferredAudio.CurrentItem?.Value == StreamContainerOption.None)
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
            var queryingUri = new Uri(_downloadUri, string.Empty);
            Video? info;
            StreamManifest? streams = null;
            try
            {
                // Keep on main thread so we can update UI.
                // Run concurrently and display title as soon as available.
                var t1 = _downloadManager.QueryVideoAsync(_downloadUri).ConfigureAwait(true);
                var t2 = _downloadManager.QueryStreamInfoAsync(_downloadUri).ConfigureAwait(true);
                info = await t1;
                VideoTitle = info.Title;
                SetError();
                streams = await t2;
            }
            catch (TaskCanceledException) { VideoTitle = string.Empty; }
            catch (HttpRequestException) { SetError(Resources.ConnectionFailed); }
            catch (UriFormatException) { SetError(Resources.InvalidUrl); }
            if (_downloadUri != queryingUri)
            {
                // If we query multiple times, ignore the previous outdated requests.
                return;
            }

            // Display.
            if (streams != null)
            {
                var query = _streamSelector.SelectStreams(streams, true, true, GetDownloadOptions());
                if (query.Video != null || query.Audio != null)
                {
                    IsDownloadValid = true;

                    if (query.Video is MuxedStreamInfo)
                    {
                        VideoStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0} - {1} ({2:N1}mb) (with audio)",
                            query.Video.VideoCodec,
                            query.Video.VideoQuality.Label,
                            query.Video.Size.MegaBytes);
                        AudioStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0}",
                            query.Audio?.AudioCodec);
                    }
                    else
                    {
                        if (query.Video != null)
                        {
                            VideoStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0} - {1} ({2:N1}mb)",
                                query.Video.VideoCodec,
                                query.Video.VideoQuality.Label,
                                query.Video.Size.MegaBytes);
                        }
                        if (query.Audio != null)
                        {
                            AudioStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0} - {1:N0}kbps ({2:N1}mb)",
                                query.Audio.AudioCodec,
                                query.Audio.Bitrate.KiloBitsPerSecond,
                                query.Audio.Size.MegaBytes);
                        }
                    }
                    _downloadCommand?.RaiseCanExecuteChanged();
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

        public ICommand DownloadCommand => CommandHelper.InitCommand(ref _downloadCommand, OnDownload, () => CanDownload);
        private RelayCommand? _downloadCommand;
        private bool CanDownload => IsDownloadValid && !IsDownloadInitializing;
        private async void OnDownload()
        {
            if (!CanDownload) { return; }

            IsDownloadInitializing = true;
            _downloadCommand?.RaiseCanExecuteChanged();

            StreamManifest? streams = null;
            try
            {
                streams = await _downloadManager.QueryStreamInfoAsync(_downloadUri!).ConfigureAwait(true);
            }
            catch (TaskCanceledException) { VideoTitle = string.Empty; }
            catch (HttpRequestException) { SetError(Resources.ConnectionFailed); }
            catch (UriFormatException) { SetError(Resources.InvalidUrl); }

            IsDownloadInitializing = false;
            _downloadCommand?.RaiseCanExecuteChanged();

            if (streams != null)
            {
                var query = _downloadManager.SelectStreams(streams, DownloadVideo, DownloadAudio, GetDownloadOptions());

                var dialogOptions = new SaveFileDialogSettings()
                {
                    Filter = string.Format(CultureInfo.InvariantCulture, "Video files (*.{0})|*.{0}|All files (*.*)|*.*", query.FileExtension),
                    OverwritePrompt = true
                };

                if (_dialogService.ShowSaveFileDialog(this, dialogOptions) == true)
                {
                    var destination = dialogOptions.FileName;
                    if (!string.IsNullOrEmpty(destination))
                    {
                        await _downloadManager.DownloadAsync(query, destination, null).ConfigureAwait(false);
                    }
                }
            }
        }

        private DownloadOptions GetDownloadOptions()
        {
            return new DownloadOptions()
            {
                PreferredVideo = PreferredVideo.CurrentItem!.Value,
                PreferredAudio = PreferredAudio.CurrentItem!.Value,
                MaxQuality = MaxQuality.CurrentItem!.Value,
                ConcurrentDownloads = 2
            };
        }

        public bool DisplayError => !DisplayDownloadInfo && Message != null;
        public bool HasDownloads => Downloads.Any();

        private void SetError(string text = "")
        {
            DisplayDownloadInfo = string.IsNullOrEmpty(text);
            Message = text;
            RaisePropertyChanged(nameof(DisplayError));
        }

        private void DownloadManager_DownloadAdded(object sender, DownloadTaskEventArgs e)
        {
            Downloads.Add(new DownloadItem(e.Download, VideoTitle));
            RaisePropertyChanged(nameof(HasDownloads));
        }
    }
}
