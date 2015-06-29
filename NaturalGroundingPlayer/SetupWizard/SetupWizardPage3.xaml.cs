using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Business;
using DataAccess;

namespace NaturalGroundingPlayer {
    public partial class SetupWizardPage3 : Page {
        public SetupWizardPage3() {
            InitializeComponent();
        }

        private SetupWizard owner;

        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            owner = (SetupWizard)Window.GetWindow(this);

            MpcConfigBusiness.IsSvpEnabled = true;
            MpcConfigBusiness.IsMadvrEnabled = false;
            MpcConfigBusiness.IsWidescreenEnabled = false;

            MediaList.Settings.SetCondition(FieldConditionEnum.IsInDatabase, true);
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
                    // File exists, play.
                    await owner.Player.PlayVideoAsync(PlayerAccess.GetVideoByFileName(VideoInfo.FileName));
                } else if (VideoInfo.HasDownloadUrl) {
                    // File doesn't exist, download.
                    await SessionCore.Instance.Business.DownloadManager.DownloadVideoAsync(PlayerAccess.GetVideoById(VideoInfo.MediaId.Value), -1, DownloadBusiness_DownloadCompleted);
                }
            }
        }

        private async void DownloadBusiness_DownloadCompleted(object sender, DownloadCompletedEventArgs e) {
            if (e.DownloadInfo.IsCompleted) {
                MediaList.business.RefreshPlaylist(e.DownloadInfo.Request, null);
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
    }
}
