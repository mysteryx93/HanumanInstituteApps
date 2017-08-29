using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EmergenceGuardian.WpfCommon;
using EmergenceGuardian.Downloader;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using EmergenceGuardian.FFmpeg;
using Business;
using DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window {
        private Media video;
        private VideoInfo downloadInfo;
        private BestFormatInfo bestFormat;
        private FFmpegProcess localInfo;
        private DownloadOptions options;
        private WindowHelper helper;

        public static DownloadWindow Instance(Media video) {
            DownloadWindow NewForm = new DownloadWindow();
            NewForm.video = video;
            SessionCore.Instance.Windows.ShowDialog(NewForm);
            return NewForm;
        }

        public DownloadWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            FFmpegConfig.FFmpegPath = "Encoder/ffmpeg.exe";
            FFmpegConfig.UserInterfaceManager = new FFmpegUserInterfaceManager(this);
            GridLocalFile.Visibility = Visibility.Hidden;

            List<ListItem<int>> DownloadQualityList = new List<ListItem<int>>();
            DownloadQualityList.Add(new ListItem<int>("Max", 0));
            DownloadQualityList.Add(new ListItem<int>("1080p", 1080));
            DownloadQualityList.Add(new ListItem<int>("720p", 720));
            DownloadQualityList.Add(new ListItem<int>("480p", 480));
            DownloadQualityList.Add(new ListItem<int>("360p", 360));
            DownloadQualityList.Add(new ListItem<int>("240p", 240));
            MaxDownloadQualityCombo.ItemsSource = DownloadQualityList;
            MaxDownloadQualityCombo.SelectedIndex = 0;

            await QueryAsync();
            await QueryLocalAsync();
            StreamOption_Click(this, e);
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e) {
            await QueryAsync();
        }

        private async Task QueryLocalAsync() {
            if (video == null || video.FileName == null)
                return;

            string SrcFile = Settings.NaturalGroundingFolder + video.FileName;
            if (File.Exists(SrcFile)) {
                DownloadPlaylistBusiness Business = new DownloadPlaylistBusiness();
                var IsHigher = await Business.IsHigherQualityAvailable(bestFormat, Settings.NaturalGroundingFolder + video.FileName);
                if (Business.LastInfoReader != null) {
                    localInfo = Business.LastInfoReader;
                    long LocalFileSize = new FileInfo(SrcFile).Length;
                    if (localInfo?.VideoStream != null)
                        LocalVideoText.Text = string.Format("{0} {1}p ({2}mb)", FirstCharToUpper(localInfo.VideoStream.Format), localInfo.VideoStream.Height, LocalFileSize / 1024 / 1024);
                    if (localInfo.AudioStream != null)
                        LocalAudioText.Text = string.Format("{0} {1}", localInfo.AudioStream.Format, localInfo.AudioStream.Bitrate > 0 ? localInfo.AudioStream.Bitrate.ToString() + "kbps" : "");
                    LocalVideoText.Visibility = localInfo.VideoStream != null ? Visibility.Visible : Visibility.Hidden;
                    LocalAudioText.Visibility = localInfo.AudioStream != null ? Visibility.Visible : Visibility.Hidden;

                    if (IsHigher == VideoListItemStatusEnum.BetterAudioAvailable)
                        LocalVideoOption.IsChecked = true;
                    else if (IsHigher == VideoListItemStatusEnum.BetterVideoAvailable)
                        LocalAudioOption.IsChecked = true;
                    else if (IsHigher == VideoListItemStatusEnum.OK) {
                        LocalVideoOption.IsChecked = true;
                        LocalAudioOption.IsChecked = true;
                    }

                    GridLocalFile.Visibility = Visibility.Visible;
                }
            }
            DownloadButton.IsEnabled = true;
        }

        private string FirstCharToUpper(string input) {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private async Task QueryAsync() {
            QueryButton.IsEnabled = false;
            DownloadButton.IsEnabled = false;
            downloadInfo = await DownloadManager.GetDownloadInfoAsync(video.DownloadUrl);
            if (downloadInfo != null) {
                TitleTextBox.Text = downloadInfo.Title;
                options = new DownloadOptions() {
                    PreferredFormat = (SelectStreamFormat)PreferredFormatCombo.SelectedIndex,
                    PreferredAudio = (SelectStreamFormat)PreferredAudioCombo.SelectedIndex,
                    MaxQuality = (int)MaxDownloadQualityCombo.SelectedValue,
                    SimultaneousDownloads = 2
                };
                BestFormatInfo StreamInfo = DownloadManager.SelectBestFormat(downloadInfo, options);
                DownloadVideoText.Text = GetStreamDescription(StreamInfo.BestVideo);
                DownloadAudioText.Text = GetStreamDescription(StreamInfo.BestAudio);
                bestFormat = StreamInfo;
                DownloadButton.IsEnabled = true;
            } else {
                TitleTextBox.Text = "Invalid URL";
            }
            QueryButton.IsEnabled = true;
        }

        private string GetStreamDescription(MediaStreamInfo stream) {
            if (stream is VideoStreamInfo) {
                VideoStreamInfo VStream = stream as VideoStreamInfo;
                return string.Format("{0} {1}p ({2}mb)", VStream.VideoEncoding, DownloadManager.GetVideoHeight(VStream), VStream.ContentLength / 1024 / 1024);
            } else if (stream is AudioStreamInfo) {
                AudioStreamInfo AStream = stream as AudioStreamInfo;
                return string.Format("{0} {1}kbps ({2}mb)", AStream.AudioEncoding, AStream.Bitrate / 1024, AStream.ContentLength / 1024 / 1024);
            } else if (stream is MixedStreamInfo) {
                MixedStreamInfo MStream = stream as MixedStreamInfo;
                return string.Format("{0} {1}p ({2}mb) (with audio)", MStream.VideoEncoding, DownloadManager.GetVideoHeight(MStream), MStream.ContentLength / 1024 / 1024);
            } else
                return "";
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e) {
            if (downloadInfo == null)
                return;

            this.Close();
            DownloadAction Action = LocalAudioOption.IsChecked == true ? DownloadAction.DownloadVideo : LocalVideoOption.IsChecked == true ? DownloadAction.DownloadAudio : DownloadAction.Download;
            await SessionCore.Instance.Business.DownloadManager.DownloadVideoAsync(video, -1, null, Action, options);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void StreamOption_Click(object sender, RoutedEventArgs e) {
            string VFormat = DownloadVideoOption.IsChecked == true ? DownloadManager.GetVideoEncoding(bestFormat.BestVideo).ToString() : localInfo?.VideoStream?.Format;
            string AFormat = DownloadAudioOption.IsChecked == true ? bestFormat.BestAudio?.AudioEncoding.ToString() : localInfo?.AudioStream?.Format;
            ContainerText.Text = DownloadBusiness.GetFinalExtension(VFormat, AFormat);
            DownloadButton.IsEnabled = DownloadVideoOption.IsChecked == true || DownloadAudioOption.IsChecked == true;
        }
    }
}
