using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EmergenceGuardian.WpfCommon;
using EmergenceGuardian.Downloader;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using PropertyChanged;
using EmergenceGuardian.FFmpeg;
using Business;

namespace EmergenceGuardian.YangYouTubeDownloader {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DownloadManager Manager = new DownloadManager();
        public VideoInfo Info;
        public string VideoUrl;
        DownloadOptions Options;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            AppPaths.ConfigureFFmpegPaths(this);

            GridInfo.Visibility = Visibility.Hidden;
            DownloadsView.Visibility = Visibility.Hidden;
            DownloadUrl.Focus();
            DownloadsView.ItemsSource = Manager.DownloadsList;

            List<ListItem<int>> DownloadQualityList = new List<ListItem<int>>();
            DownloadQualityList.Add(new ListItem<int>("Max", 0));
            DownloadQualityList.Add(new ListItem<int>("1080p", 1080));
            DownloadQualityList.Add(new ListItem<int>("720p", 720));
            DownloadQualityList.Add(new ListItem<int>("480p", 480));
            DownloadQualityList.Add(new ListItem<int>("360p", 360));
            DownloadQualityList.Add(new ListItem<int>("240p", 240));
            MaxDownloadQualityCombo.ItemsSource = DownloadQualityList;
            MaxDownloadQualityCombo.SelectedIndex = 0;

            SplashWindow F = SplashWindow.Instance(this, Properties.Resources.AppIcon);
            F.CanClose();
            F.ShowDialog();
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e) {
            GridInfo.Visibility = Visibility.Visible;
            ErrorText.Visibility = Visibility.Hidden;
            QueryButton.IsEnabled = false;
            DownloadButton.IsEnabled = false;
            VideoUrl = DownloadUrl.Text;
            TitleText.Text = "";
            VideoText.Text = "";
            AudioText.Text = "";
            ContainerText.Text = "";
            Info = await DownloadManager.GetDownloadInfoAsync(VideoUrl);
            if (Info != null) {
                TitleText.Text = Info.Info.Title;
                TitleText.ToolTip = Info.Info.Title;
                GridInfo.Visibility = Visibility.Visible;
                Options = new DownloadOptions() {
                    PreferredFormat = (SelectStreamFormat)PreferredFormatCombo.SelectedIndex,
                    PreferredAudio = (SelectStreamFormat)PreferredAudioCombo.SelectedIndex,
                    MaxQuality = (int)MaxDownloadQualityCombo.SelectedValue,
                    SimultaneousDownloads = 2
                };
                BestFormatInfo StreamInfo = DownloadManager.SelectBestFormat(Info.Streams, Options);
                VideoText.Text = GetStreamDescription(StreamInfo.BestVideo);
                AudioText.Text = GetStreamDescription(StreamInfo.BestAudio);
                ContainerText.Text = DownloadManager.GetFinalExtension(StreamInfo.BestVideo, StreamInfo.BestAudio);
                DownloadButton.IsEnabled = true;
            } else {
                GridInfo.Visibility = Visibility.Hidden;
                ErrorText.Visibility = Visibility.Visible;
            }
            QueryButton.IsEnabled = true;
        }

        private string GetStreamDescription(MediaStreamInfo stream) {
            if (stream is VideoStreamInfo) {
                VideoStreamInfo VStream = stream as VideoStreamInfo;
                return string.Format("{0} {1}p ({2}mb)", VStream.VideoEncoding, DownloadManager.GetVideoHeight(VStream), VStream.Size / 1024 / 1024);
            } else if (stream is AudioStreamInfo) {
                AudioStreamInfo AStream = stream as AudioStreamInfo;
                return string.Format("{0} {1}kbps ({2}mb)", AStream.AudioEncoding, AStream.Bitrate / 1024, AStream.Size / 1024 / 1024);
            } else if (stream is MuxedStreamInfo) {
                MuxedStreamInfo MStream = stream as MuxedStreamInfo;
                return string.Format("{0} {1}p ({2}mb) (with audio)", MStream.VideoEncoding, DownloadManager.GetVideoHeight(MStream), MStream.Size / 1024 / 1024);
            } else
                return "";
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e) {
            if (Info == null)
                return;

            string Filter = string.Format("Video files (*{0})|*{0}|All files (*.*)|*.*", ContainerText.Text);
            string Destination = FileFolderDialog.ShowSaveFileDialog(null, Filter);
            if (!string.IsNullOrEmpty(Destination)) {
                DownloadsView.Visibility = Visibility.Visible;
                await Manager.DownloadVideoAsync(VideoUrl, Destination, TitleText.Text, null, DownloadAction.Download, Options, null);
            }
        }
    }
}
