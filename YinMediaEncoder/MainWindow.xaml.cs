using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Business;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Navigation;
using NaturalGroundingPlayer;
using DataAccess;
using System.Windows.Controls;

namespace YinMediaEncoder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private WindowHelper helper;
        private WmpPlayerWindow playerOriginal;
        private WmpPlayerWindow playerChanges;
        private MediaEncoderSettings encodeSettings = new MediaEncoderSettings();
        private MediaEncoderBusiness business = new MediaEncoderBusiness();
        private bool isBinding = false;

        public MainWindow() {
            InitializeComponent();
            SessionCore.Instance.Start(this);
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            this.DataContext = encodeSettings;
            SettingsTab.Items.CurrentChanging += new CurrentChangingEventHandler(Items_CurrentChanging);
            if (MediaPlayer.WindowsMediaPlayer.IsWmpInstalled) {
                playerOriginal = new WmpPlayerWindow();
                playerOriginal.Title = "Original";
                playerOriginal.WindowState = WindowState.Maximized;
                playerChanges = new WmpPlayerWindow();
                playerChanges.Title = "Preview Changes";
                playerChanges.WindowState = WindowState.Maximized;
                playerChanges.Player.Player.PositionChanged += Player_PositionChanged;
            } else {
                PreviewOriginalButton.Visibility = Visibility.Hidden;
                PreviewChangesButton.Visibility = Visibility.Hidden;
            }
            if (Settings.SavedFile.MediaPlayerApp != MediaPlayerApplication.Mpc)
                PreviewMpcButton.Visibility = Visibility.Hidden;
            business.EncodingCompleted += business_EncodingCompleted;
            ProcessingQueueList.ItemsSource = MediaEncoderBusiness.ProcessingQueue;
            MpcConfigBusiness.IsSvpEnabled = false;

            MediaEncoderSettings RecoverSettings = await business.AutoLoadPreviewFileAsync();
            if (RecoverSettings != null)
                SetEncodeSettings(RecoverSettings);

            encodeSettings.AutoCalculateSize = true;

            // Only recover jobs if there are no jobs running
            if (!MediaEncoderBusiness.ProcessingQueue.Any())
                await business.AutoLoadJobsAsync();
        }

        /// <summary>
        /// When position changes in PreviewChanges and both players are on pause, the Original will go to the same position.
        /// </summary>
        private void Player_PositionChanged(object sender, EventArgs e) {
            MediaPlayer.WindowsMediaPlayer Player1 = playerChanges.Player.Player;
            MediaPlayer.WindowsMediaPlayer Player2 = playerOriginal.Player.Player;
            if (Player1 != null && Player2 != null && playerOriginal.IsVisible && playerChanges.IsVisible && !Player1.IsPlaying && !Player2.IsPlaying)
                Player2.SetFramePosition(encodeSettings.TrimStart ?? 0 + Player1.Position + .195);
        }

        private void business_EncodingCompleted(object sender, EncodingCompletedEventArgs e) {
            CompletedWindow.Instance(e);
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            playerOriginal?.Player?.Close();
            playerChanges?.Player?.Close();
            await business.DeletePreviewFilesAsync();
        }

        private void Window_Activated(object sender, EventArgs e) {
            if (playerChanges?.Player.Position > 0)
                encodeSettings.Position = playerChanges.Player.Position;
        }

        private async void SelectVideoButton_Click(object sender, RoutedEventArgs e) {
            string SelectedFile = null;
            string DisplayName = null;
            if (SelectVideoButton.Content == MenuSelectFromPlaylist.Header) {
                VideoListItem PlaylistItem = SearchVideoWindow.Instance(new SearchSettings() {
                    MediaType = MediaType.Video,
                    ConditionField = FieldConditionEnum.FileExists,
                    ConditionValue = BoolConditionEnum.Yes,
                    RatingCategory = "Height",
                    RatingOperator = OperatorConditionEnum.Smaller
                });
                if (PlaylistItem != null) {
                    SelectedFile = Settings.NaturalGroundingFolder + PlaylistItem.FileName;
                    DisplayName = PlaylistItem.FileName;
                }
            } else {
                string ExtFilter = string.Format("Video Files|*{0})", string.Join(";*", Settings.VideoExtensions));
                SelectedFile = FileFolderDialog.ShowFileDialog(Settings.NaturalGroundingFolder, ExtFilter);
                DisplayName = SelectedFile;
            }
            if (!string.IsNullOrEmpty(SelectedFile)) {
                HidePreview();
                encodeSettings.AutoCalculateSize = false;
                encodeSettings.FilePath = null;
                encodeSettings.CustomScript = null;
                SettingsTab.SelectedIndex = 0;
                encodeSettings.FilePath = SelectedFile;
                encodeSettings.DisplayName = DisplayName;
                encodeSettings.CropBottom = 0;
                encodeSettings.CropLeft = 0;
                encodeSettings.CropRight = 0;
                encodeSettings.CropTop = 0;

                try {
                    await business.PreparePreviewFile(encodeSettings, true, true);
                } catch (Exception ex) {
                    MessageBox.Show(this, ex.Message, "Cannot Open File", MessageBoxButton.OK, MessageBoxImage.Error);
                    encodeSettings.FilePath = "";
                }
                encodeSettings.AutoCalculateSize = true;
            }
        }

        private void MenuSelect_Click(object sender, RoutedEventArgs e) {
            SelectVideoButton.IsOpen = false;
            SelectVideoButton.Content = ((MenuItem)sender).Header;
            SelectVideoButton_Click(sender, e);
        }

        public void SetEncodeSettings(MediaEncoderSettings value) {
            isBinding = true;
            SettingsTab.SelectedIndex = (String.IsNullOrEmpty(value.CustomScript) ? 0 : 2);
            encodeSettings = value;
            this.DataContext = value;
            isBinding = false;
        }

        public void HidePreview() {
            if (playerOriginal?.Visibility == Visibility.Visible)
                playerOriginal.Visibility = Visibility.Hidden;
            if (playerChanges?.Visibility == Visibility.Visible)
                playerChanges.Visibility = Visibility.Hidden;
        }

        private async void PreviewOriginalButton_Click(object sender, RoutedEventArgs e) {
            await PlayVideoAsync(playerOriginal, encodeSettings.FilePath);
        }

        private async void PreviewChangesButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                business.GenerateScript(encodeSettings, true, false);
                await PlayVideoAsync(playerChanges, Settings.TempFilesPath + "Preview.avs");
            }
        }

        private void PreviewMpcButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                // Show cropping borders if WMP isn't available.
                bool Preview = !MediaPlayer.WindowsMediaPlayer.IsWmpInstalled;
                business.GenerateScript(encodeSettings, Preview, false);
                MpcConfigBusiness.StartMpc(Settings.TempFilesPath + "Preview.avs");
            }
        }

        private async Task PlayVideoAsync(WmpPlayerWindow playerWindow, string fileName) {
            playerWindow.Show();
            playerWindow.Activate();
            await playerWindow.Player.OpenFileAsync(fileName);
            playerWindow.Player.MediaOpened += (s2, e2) => {
                if (encodeSettings.Position.HasValue)
                    playerWindow.Player.Position = encodeSettings.Position.Value;
                playerWindow.Player.Player.Pause();
            };
        }

        private async void EncodeButton_Click(object sender, RoutedEventArgs e) {
            if (!Validate())
                return;

            MediaEncoderSettings EncodeSettings = encodeSettings;
            try {
                HidePreview();
                SetEncodeSettings((MediaEncoderSettings)encodeSettings.Clone());
                encodeSettings.FilePath = "";
                await Task.Delay(100); // Wait for media player file to be released.
                await business.EncodeFileAsync(EncodeSettings);
            } catch (Exception ex) {
                if (!encodeSettings.ConvertToAvi || System.IO.File.Exists(Settings.TempFilesPath + "Preview.avi"))
                    SetEncodeSettings(EncodeSettings);
                MessageBox.Show(this, ex.Message, "Encoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private bool Validate() {
            bool Error = !this.IsValid() ||
                string.IsNullOrEmpty(encodeSettings.FilePath) ||
                !encodeSettings.SourceHeight.HasValue ||
                !encodeSettings.SourceWidth.HasValue ||
                !encodeSettings.SourceFrameRate.HasValue;
            if (Error)
                MessageBox.Show(this, "You must enter required file information.", "Validation Error");
            return !Error;
        }

        private async void ConvertToAviCheckbox_Click(object sender, RoutedEventArgs e) {
            if (isBinding)
                return;
            ConvertToAviCheckbox.IsEnabled = false;
            await business.PreparePreviewFile(encodeSettings, false, false);
            ConvertToAviCheckbox.IsEnabled = true;
        }

        /// <summary>
        /// Generates script when going to Script tab, and prevents returning to Transform tab without a confirmation to lose changes.
        /// </summary>
        private void Items_CurrentChanging(object sender, CurrentChangingEventArgs e) {
            if (string.IsNullOrEmpty(encodeSettings.FilePath))
                return;

            SettingsTab.Focus();
            var item = ((ICollectionView)sender).CurrentItem;
            bool Cancel = false;
            // Generate script when going to Script tab.
            if (SettingsTab.SelectedIndex == 2 && string.IsNullOrEmpty(encodeSettings.CustomScript)) {
                if (Validate())
                    business.GenerateCustomScript(encodeSettings);
                else
                    Cancel = true;
            } else if (SettingsTab.SelectedIndex == 0 && !string.IsNullOrEmpty(encodeSettings.CustomScript)) {
                // Ask for confirmation before going back to Transform tab and losing changes.
                if (business.CustomScriptHasChanges(encodeSettings))
                    if (MessageBox.Show("You will lose any changes to your script. Are you sure?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                        Cancel = true;
                if (!Cancel)
                    encodeSettings.CustomScript = null;
            }

            // Revert to previously-selected tab.
            if (Cancel) {
                e.Cancel = true;
                SettingsTab.SelectedItem = item;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void CalculateAudioGain_Click(object sender, RoutedEventArgs e) {
            CalculateAudioGain.IsEnabled = false;
            CalculateAudioGain.Content = "Wait";
            float? Gain = await Task.Run(() => FfmpegBusiness.GetAudioGain(encodeSettings));
            if (Gain.HasValue)
                encodeSettings.AudioGain = Gain;
            CalculateAudioGain.IsEnabled = true;
            CalculateAudioGain.Content = "Auto";
        }

        private void Codec264Option_Click(object sender, RoutedEventArgs e) {
            encodeSettings.EncodeQuality = 23;
            encodeSettings.EncodePreset = EncodePresets.veryslow;
        }

        private void Codec265Option_Click(object sender, RoutedEventArgs e) {
            encodeSettings.EncodeQuality = 22;
            encodeSettings.EncodePreset = EncodePresets.medium;
        }

        private void DeshakerGenerateButton_Click(object sender, RoutedEventArgs e) {
            DeshakerWindow.Instance(business, encodeSettings);
            //DeshakerGenerateButton.IsEnabled = false;
            //await business.GenerateDeshakerLog(encodeSettings, business.GetPreviewSourceFile(encodeSettings));
            //DeshakerGenerateButton.IsEnabled = true;
        }
    }
}
