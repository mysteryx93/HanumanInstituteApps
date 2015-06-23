using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Business;
using DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for SetupWizard.xaml
    /// </summary>
    public partial class SetupWizard : Window {
        public static void Instance(Action callback) {
            SetupWizard NewForm = new SetupWizard();
            NewForm.callback = callback;
            SessionCore.Instance.Windows.Show(NewForm);
        }

        private WindowHelper helper;
        private int currentPage = 1;
        private int totalPages = 7;
        private int? downloadPage;
        protected Action callback;
        public IMediaPlayerBusiness Player;

        public SetupWizard() {
            InitializeComponent();
            helper = new WindowHelper(this);
            Player = SessionCore.Instance.GetNewPlayer();
            Player.SetPath();
            Player.AllowClose = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DisplayPage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) {
            currentPage++;
            DisplayPage();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e) {
            currentPage--;
            DisplayPage();
        }

        private void DisplayPage() {
            PreviousButton.IsEnabled = (currentPage > 1);
            NextButton.Content = (currentPage == totalPages ? "_Finish" : "_Next");
            ProgressText.Text = string.Format("Page {0} / {1}", currentPage, totalPages);
            downloadPage = null;
            if (currentPage == 1)
                ContentFrame.Navigate(new SetupWizardPage1());
            else if (currentPage == 2)
                ContentFrame.Navigate(new SetupWizardPage2());
            else if (currentPage == 3)
                ContentFrame.Navigate(new SetupWizardPage3());
            else if (currentPage == 4)
                ContentFrame.Navigate(new SetupWizardPage4());
            else if (currentPage == 5)
                ContentFrame.Navigate(new SetupWizardPage5());
            else if (currentPage == 6)
                ContentFrame.Navigate(new SetupWizardPage6());
            else if (currentPage == 7)
                ContentFrame.Navigate(new SetupWizardPage7());
            else if (currentPage > totalPages)
                this.Close();
        }

        public async Task DownloadAndPlaySample(string artist, string title) {
            EditVideoBusiness FindVideoBusiness = new EditVideoBusiness();
            Media sample = FindVideoBusiness.GetVideoByTitle(MediaType.Video, artist, title);
            if (sample != null) {
                if (sample.FileName != null && File.Exists(Settings.NaturalGroundingFolder + sample.FileName)) {
                    // File exists, play.
                    await PlayVideo(sample);
                } else if (sample.DownloadUrl != null) {
                    // File doesn't exist, download.
                    // It will only auto-play if user is still on the same page.
                    downloadPage = currentPage;
                    await SessionCore.Instance.Business.DownloadManager.DownloadVideoAsync(sample, -1, DownloadBusiness_DownloadCompleted);
                }
            }
        }

        private async void DownloadBusiness_DownloadCompleted(object sender, DownloadCompletedEventArgs e) {
            if (e.DownloadInfo.IsCompleted && this.IsVisible && currentPage == downloadPage)
                await PlayVideo(PlayerAccess.GetVideoById(e.DownloadInfo.Request.MediaId));
        }

        private async Task PlayVideo(Media video) {
            await Player.PlayVideoAsync(video);
            await Task.Delay(500);
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Player.Close();
            MpcConfigBusiness.IsSvpEnabled = Settings.SavedFile.EnableSvp;
            MpcConfigBusiness.IsMadvrEnabled = Settings.SavedFile.EnableMadVR;
            MpcConfigBusiness.IsWidescreenEnabled = Settings.SavedFile.Widescreen;
            if (callback != null)
                callback();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
