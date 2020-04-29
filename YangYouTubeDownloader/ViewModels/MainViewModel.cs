using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using YoutubeExplode.Videos.Streams;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Downloads;
using HanumanInstitute.FFmpeg;
using PropertyChanged;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using System.Net.Http;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.YangYouTubeDownloader.Models;
using System.Collections.ObjectModel;
using YoutubeExplode.Videos;
using HanumanInstitute.YangYouTubeDownloader.Properties;

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

            var prefV = PreferredVideo.List;
            prefV.Add(SelectStreamFormat.Best);
            prefV.Add(SelectStreamFormat.MP4);
            prefV.Add(SelectStreamFormat.WebM);
            prefV.Add(SelectStreamFormat.Tgpp);
            prefV.Add(SelectStreamFormat.None);
            PreferredVideo.SelectedIndex = 0;

            var prefA = PreferredAudio.List;
            prefA.Add(SelectStreamFormat.Best);
            prefA.Add(SelectStreamFormat.MP4);
            prefA.Add(SelectStreamFormat.WebM);
            prefA.Add(SelectStreamFormat.Tgpp);
            prefA.Add(SelectStreamFormat.None);
            PreferredAudio.SelectedIndex = 0;

            var qual = MaxQuality.List;
            qual.Add(0, Resources.Max);
            foreach (var res in new[] { 4320, 3072, 2160, 1440, 1080, 720, 480, 360, 240, 144 })
            {
                qual.Add(res, string.Format(CultureInfo.InvariantCulture, "{0}p", res));
            }
            MaxQuality.SelectedIndex = 0;

            _downloadManager.DownloadAdded += DownloadManager_DownloadAdded;
        }

#pragma warning disable CA1056 // Uri properties should not be strings
        public string DownloadUrl { get; set; } = "https://www.youtube.com/watch?v=4OqXWzekVw4"; // string.Empty;
#pragma warning restore CA1056 // Uri properties should not be strings
        public bool DisplayDownloadInfo { get; private set; }
        public bool IsDownloadValid { get; private set; }
        public ISelectableList<ListItem<SelectStreamFormat>> PreferredVideo { get; private set; } = new SelectableList<ListItem<SelectStreamFormat>>();
        public ISelectableList<ListItem<SelectStreamFormat>> PreferredAudio { get; private set; } = new SelectableList<ListItem<SelectStreamFormat>>();
        public ISelectableList<ListItem<int>> MaxQuality { get; private set; } = new SelectableList<ListItem<int>>();
        //public ObservableCollection<DownloadTaskInfo> ActiveDownloads => downloadManager.DownloadsList;
        // public bool IsQuerying { get; private set; } = false;
        public string Message { get; private set; } = string.Empty;
        public string VideoTitle { get; private set; } = string.Empty;
        public string VideoContainer { get; private set; } = string.Empty;
        public string VideoStreamInfo { get; private set; } = string.Empty;
        public string AudioStreamInfo { get; private set; } = string.Empty;
        public ISelectableList<DownloadItem> Downloads { get; private set; } = new SelectableList<DownloadItem>();
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
            VideoContainer = string.Empty;

            // Validate.
            if (PreferredVideo.SelectedItem?.Value == SelectStreamFormat.None && PreferredAudio.SelectedItem?.Value == SelectStreamFormat.None)
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
                var options = GetDownloadOptions();
                var bestVideo = _streamSelector.SelectBestVideo(streams, options);
                var bestAudio = _streamSelector.SelectBestAudio(streams, options);
                if (bestVideo != null || bestAudio != null)
                {
                    IsDownloadValid = true;

                    if (bestVideo is MuxedStreamInfo)
                    {
                        VideoStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0} - {1} ({2:N1}mb) (with audio)",
                            bestVideo.VideoCodec,
                            bestVideo.VideoQualityLabel,
                            bestVideo.Size.TotalMegaBytes);
                        AudioStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0}",
                            bestAudio?.AudioCodec);
                        VideoContainer = _streamSelector.GetFinalExtension(bestVideo, null).ToUpperInvariant();
                    }
                    else
                    {
                        if (bestVideo != null)
                        {
                            VideoStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0} - {1} ({2:N1}mb)",
                                bestVideo.VideoCodec,
                                bestVideo.VideoQualityLabel,
                                bestVideo.Size.TotalMegaBytes);
                        }
                        if (bestAudio != null)
                        {
                            AudioStreamInfo = string.Format(CultureInfo.InvariantCulture, "{0} - {1:N0}kbps ({2:N1}mb)",
                                bestAudio.AudioCodec,
                                bestAudio.Bitrate.KiloBitsPerSecond,
                                bestAudio.Size.TotalMegaBytes);
                        }
                        VideoContainer = _streamSelector.GetFinalExtension(bestVideo, bestAudio).ToUpperInvariant();
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
        private bool CanDownload => IsDownloadValid;
        private async void OnDownload()
        {
            if (!CanDownload) { return; }

            var dialogOptions = new SaveFileDialogSettings()
            {
                Filter = string.Format(CultureInfo.InvariantCulture, "Video files (*{0})|*{0}|All files (*.*)|*.*", VideoContainer)
            };

            if (_dialogService.ShowSaveFileDialog(this, dialogOptions) == true)
            {
                var destination = dialogOptions.FileName;
                if (!string.IsNullOrEmpty(destination))
                {
                    await _downloadManager.DownloadAsync(_downloadUri!, destination, options: GetDownloadOptions()).ConfigureAwait(false);
                }
            }
        }

        private DownloadOptions GetDownloadOptions()
        {
            return new DownloadOptions()
            {
                PreferredVideo = PreferredVideo.SelectedItem.Value,
                PreferredAudio = PreferredAudio.SelectedItem.Value,
                MaxQuality = MaxQuality.SelectedItem.Value,
                ConcurrentDownloads = 2
            };
        }

        public bool DisplayError => !DisplayDownloadInfo && Message != null;
        public bool HasDownloads => Downloads.List.Any();

        private void SetError(string text = "")
        {
            DisplayDownloadInfo = string.IsNullOrEmpty(text);
            Message = text;
            RaisePropertyChanged(nameof(DisplayError));
        }

        private void DownloadManager_DownloadAdded(object sender, DownloadTaskEventArgs e)
        {
            Downloads.List.Add(new DownloadItem(e.Download, VideoTitle));
            RaisePropertyChanged(nameof(HasDownloads));
        }
    }
}
