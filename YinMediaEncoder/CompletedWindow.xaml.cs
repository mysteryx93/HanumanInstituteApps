using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NaturalGroundingPlayer;
using Business;
using DataAccess;

namespace YinMediaEncoder {
    /// <summary>
    /// Interaction logic for CompletedWindow.xaml
    /// </summary>
    public partial class CompletedWindow : Window {
        public static void Instance(EncodingCompletedEventArgs jobInfo) {
            CompletedWindow NewForm = new CompletedWindow();
            NewForm.jobInfo = jobInfo;
            NewForm.Show();
        }

        protected EncodingCompletedEventArgs jobInfo;
        private WindowHelper helper;
        IMediaPlayerBusiness player = SessionCore.Instance.GetNewPlayer();

        public CompletedWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            player.SetPath();
            DataContext = jobInfo;
            if (!Settings.SavedFile.EnableSvp)
                EnableSvpCheckBox.Visibility = Visibility.Hidden;
            if (!Settings.SavedFile.EnableMadVR)
                EnableMadVrCheckBox.Visibility = Visibility.Hidden;
        }

        private async void PlayOldButton_Click(object sender, RoutedEventArgs e) {
            MpcConfigBusiness.IsSvpEnabled = EnableSvpCheckBox.IsChecked.Value;
            MpcConfigBusiness.IsMadvrEnabled = EnableMadVrCheckBox.IsChecked.Value;
            await PlayVideoAsync(jobInfo.OldFileName);
        }

        private async void PlayNewButton_Click(object sender, RoutedEventArgs e) {
            MpcConfigBusiness.IsSvpEnabled = EnableSvpCheckBox.IsChecked.Value;
            MpcConfigBusiness.IsMadvrEnabled = EnableMadVrCheckBox.IsChecked.Value;
            await PlayVideoAsync(jobInfo.NewFileName);
        }

        private async Task PlayVideoAsync(string fileName) {
            await player.PlayVideoAsync(fileName);
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e) {
            try {
                OkButton.IsEnabled = false;
                player.Close();
                MediaEncoderBusiness business = new MediaEncoderBusiness();
                if (ReplaceOption.IsChecked.Value) {
                    await Task.Run(() => business.FinalizeReplace(jobInfo));
                } else if (KeepOption.IsChecked.Value) {
                    await Task.Run(() => business.FinalizeKeep(jobInfo));
                }
                if (ReencodeCheckbox.IsChecked.Value) {
                    MainWindow ActiveWindow = SessionCore.Instance.Windows.Current as MainWindow;
                    
                    // If Media Encoder is open, close preview windows and replace preview files.
                    // Otherwise, open Media Encoder and pre-load preview files.
                    if (ActiveWindow != null)
                        ActiveWindow.HidePreview();
                    await business.MovePreviewFilesAsync(jobInfo.Settings);

                    if (ActiveWindow != null) {
                        // Media Encoder is already open.
                        ActiveWindow.SetEncodeSettings(jobInfo.Settings);
                    }
                }
                business.DeleteJobFiles(jobInfo.Settings);
                jobInfo.Settings.JobIndex = -1;
                this.Close();
            }
            catch (Exception ex) {
                OkButton.IsEnabled = true;
                MessageBox.Show(this, ex.Message, "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Option_Click(object sender, RoutedEventArgs e) {
            OkButton.IsEnabled = true;
        }
    }
}
