using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Business;
using DataAccess;
using EmergenceGuardian.Downloader;

namespace NaturalGroundingPlayer {
    public partial class SetupWizardPage5 : Page {
        public SetupWizardPage5() {
            InitializeComponent();
        }

        private SetupWizard owner;

        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            owner = (SetupWizard)Window.GetWindow(this);

            MpcConfigBusiness.IsSvpEnabled = true;
            MpcConfigBusiness.IsMadvrEnabled = true;

            MediaList.Settings.IsInDatabase = true;
            await Task.Delay(100);
            ComboBox_SelectionChanged(null, null);
        }

        private void MediaList_ItemDoubleClick(object sender, EventArgs e) {
            PlayButton_Click(null, null);
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e) {
            VideoListItem VideoInfo = MediaList.SelectedItem;
            if (VideoInfo != null) {
                if (VideoInfo.FileName != null && VideoInfo.FileExists) {
                    MpcConfigBusiness.IsWidescreenEnabled = WidescreenCheckBox.IsChecked.Value;
                    // File exists, play.
                    await owner.Player.PlayVideoAsync(PlayerAccess.GetVideoByFileName(VideoInfo.FileName), false);
                } else if (VideoInfo.HasDownloadUrl) {
                    // File doesn't exist, download.
                    await SessionCore.Instance.Business.DownloadManager.DownloadVideoAsync(PlayerAccess.GetVideoById(VideoInfo.MediaId.Value), -1, DownloadBusiness_DownloadCompleted);
                }
            }
        }

        private async void DownloadBusiness_DownloadCompleted(object sender, DownloadCompletedEventArgs e) {
            DownloadItemData IData = e.DownloadInfo.Data as DownloadItemData;
            if (e.DownloadInfo.IsCompleted) {
                MediaList.business.RefreshPlaylist(IData.Media, null);
                await MediaList.LoadDataAsync();
            }
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!IsLoaded)
                return;

            string Sel;
            if (e != null)
                Sel = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            else
                Sel = ResolutionCombo.Text;
            if (Sel != "> 1080p") {
                int SelHeight = 0;
                if (Sel == "288p")
                    SelHeight = 288;
                else if (Sel == "360p")
                    SelHeight = 360;
                else if (Sel == "480p")
                    SelHeight = 480;
                else if (Sel == "720p")
                    SelHeight = 720;
                else // 1080p
                    SelHeight = 1080;
                MediaList.Settings.SetRatingCategory("Height", OperatorConditionEnum.Equal, SelHeight);
            } else
                MediaList.Settings.SetRatingCategory("Height", OperatorConditionEnum.GreaterOrEqual, 1081);
            await MediaList.LoadDataAsync();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(
@"if (targetWidth <= 1366) && (targetHeight <= 768) && (srcHeight <= 300) 'Laptop 288p'
else if (targetWidth <= 1366) && (targetHeight <= 768) && (srcHeight <= 480) 'Laptop 480p'
else if (targetWidth <= 1366) && (targetHeight <= 768) && (srcHeight <= 720) 'Laptop 720p'
else if (targetWidth <= 1366) && (targetHeight <= 768) 'Laptop 1080p'
else if (srcHeight <= 300) 'TV 288p'
else if (srcHeight <= 480) 'TV 480p'
else if (srcHeight <= 720) 'TV 720p'
else 'TV 1080p'");
        }
    }
}
