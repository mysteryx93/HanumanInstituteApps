using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Business;
using DataAccess;
using System.Threading.Tasks;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MediaEncodingCompletedWindow.xaml
    /// </summary>
    public partial class MediaEncodingCompletedWindow : Window {
        public static void Instance(EncodingCompletedEventArgs jobInfo) {
            MediaEncodingCompletedWindow NewForm = new MediaEncodingCompletedWindow();
            NewForm.jobInfo = jobInfo;
            NewForm.Show();
        }

        protected EncodingCompletedEventArgs jobInfo;
        private WindowHelper helper;
        IMediaPlayerBusiness player = SessionCore.Instance.GetNewPlayer();

        public MediaEncodingCompletedWindow() {
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
            if (SessionCore.Instance.Business.IsStarted && (SessionCore.Instance.Windows.Current.GetType() == typeof(MainWindow) || SessionCore.Instance.Windows.Current.GetType() == typeof(ManualPlayerWindow))) {
                await SessionCore.Instance.Business.SetNextVideoFileAsync(PlayerMode.SpecialRequest, fileName);
                await SessionCore.Instance.Business.SkipVideoAsync();
            } else
                await player.PlayVideoAsync(new Media() { FileName = fileName }, false);
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
                    MediaEncoderWindow ActiveWindow = SessionCore.Instance.Windows.Current as MediaEncoderWindow;
                    
                    // If Media Encoder is open, close preview windows and replace preview files.
                    // Otherwise, open Media Encoder and pre-load preview files.
                    if (ActiveWindow != null)
                        ActiveWindow.ClosePreview();
                    await business.MovePreviewFilesAsync(jobInfo.Settings);

                    if (ActiveWindow != null) {
                        // Media Encoder is already open.
                        ActiveWindow.SetEncodeSettings(jobInfo.Settings);
                    } else {
                        // Media Encoder is not open, open it.
                        SessionCore.Instance.Windows.CloseToMain();
                        SessionCore.Instance.Menu.CommandBinding_MediaEncoder(null, null);
                    }
                }
                business.DeleteJobFiles(jobInfo.Settings);
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
