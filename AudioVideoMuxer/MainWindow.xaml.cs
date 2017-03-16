using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.WpfCommon;

namespace EmergenceGuardian.AudioVideoMuxer {
    /// <summary>
    /// Interaction logic for SetupWizard.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // private WindowHelper helper;
        private int currentPage = 1;
        private IWizardPage page;
        private bool isLastPage;
        // private int totalPages = 7;
        public MuxerBusiness Business = new MuxerBusiness();

        public MainWindow() {
            InitializeComponent();
            // helper = new WindowHelper(this);

            FFmpegConfig.FFmpegPath = "Encoder/ffmpeg.exe";
            FFmpegConfig.UserInterfaceManager = new FFmpegUserInterfaceManager(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DisplayPage();
            SplashWindow F = SplashWindow.Instance(this, Properties.Resources.AppIcon);
            F.CanClose();
            F.ShowDialog();
        }

        public int CurrentPage {
            get {
                return currentPage;
            }
            set {
                currentPage = value;
                DisplayPage();
            }
        }

        public bool IsLastPage {
            get { return isLastPage; }
            set {
                isLastPage = value;
                NextButton.Content = (value ? "_Finish" : "_Next");
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e) {
            if (page.Validate()) {
                if (currentPage == 11 || currentPage == 21 || currentPage == 31) {

                    Task run = null;
                    if (currentPage == 11)
                        run = new Task(() => Business.StartMuxe());
                    if (currentPage == 21)
                        run = new Task(() => Business.StartMerge());
                    if (currentPage == 31)
                        run = new Task(() => Business.StartSplit());
                    await WorkingWindow.InstanceAsync(this, run);
                    CurrentPage = 1;
                }

                CurrentPage++;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e) {
            if (currentPage == 10 || currentPage == 20 || currentPage == 30)
                CurrentPage = 1;
            else
                CurrentPage--;
        }

        private void DisplayPage() {
            IsLastPage = false;
            PreviousButton.IsEnabled = (currentPage > 1);
            NextButton.IsEnabled = true;
            // NextButton.Content = (currentPage == totalPages ? "_Finish" : "_Next");
            // ProgressText.Text = string.Format("Page {0} / {1}", currentPage, totalPages);
            page = null;
            if (currentPage == 1)
                page = new SetupWizardPage1();
            else if (currentPage == 10)
                page = new SetupWizardPage10();
            else if (currentPage == 11)
                page = new SetupWizardPage11();
            else if (currentPage == 20)
                page = new SetupWizardPage20();
            else if (currentPage == 21)
                page = new SetupWizardPage21();

            if (page != null) {
                page.Owner = this;
                ContentFrame.Navigate(page);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
