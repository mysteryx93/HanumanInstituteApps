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
using EmergenceGuardian.NaturalGroundingPlayer.Business;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SetupWizardPage2 : Page {
        public SetupWizardPage2() {
            InitializeComponent();
        }

        private SetupWizard owner;

        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            owner = (SetupWizard)Window.GetWindow(this);

            MpcConfiguration.IsSvpEnabled = true;
            MpcConfiguration.IsMadvrEnabled = false;
            await owner.DownloadAndPlaySample("Girls' Generation", "Sexy Dance");

            //EditVideoBusiness FindVideoBusiness = new EditVideoBusiness();
            //Media Sample = FindVideoBusiness.GetVideoByTitle(MediaType.Video, "Girls' Generation", "Sexy Dance");
            //if (Sample != null) {
            //    await DownloadAndPlaySample(Sample);
            //}
        }

        //private async Task DownloadAndPlaySample(Media sample) {
        //    if (sample.FileName != null) {
        //        // File exists, play.
        //        await owner.Player.PlayVideoAsync(sample);
        //    } else if (sample.DownloadUrl != null) {
        //        // File doesn't exist, download.
        //        await SessionCore.Instance.Business.DownloadManager.DownloadVideoAsync(sample, -1, DownloadBusiness_DownloadCompleted);
        //    }
        //}

        //private async void DownloadBusiness_DownloadCompleted(object sender, DownloadCompletedEventArgs e) {
        //    if (e.DownloadInfo.IsCompleted && this.IsVisible) {
        //        await owner.Player.PlayVideoAsync(PlayerAccess.GetVideoById(e.DownloadInfo.Request.MediaId));
        //    }
        //}
    }
}
