using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Business;
using DataAccess;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Navigation;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MediaEncoderWindow.xaml
    /// </summary>
    public partial class MediaEncoderWindow : Window {
        public static void Instance(Action callback) {
            MediaEncoderWindow NewForm = new MediaEncoderWindow();
            NewForm.callback = callback;
            SessionCore.Instance.Windows.Show(NewForm);
        }

        protected Action callback;
        private WindowHelper helper;
        private WmpPlayerWindow playerOriginal = new WmpPlayerWindow();
        private WmpPlayerWindow playerChanges = new WmpPlayerWindow();
        private MpcPlayerBusiness playerMpc = new MpcPlayerBusiness();
        private MediaEncoderSettings encodeSettings = new MediaEncoderSettings();
        private MediaEncoderBusiness business = new MediaEncoderBusiness();
        private bool isBinding = false;

        public MediaEncoderWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            this.DataContext = encodeSettings;
            SettingsTab.Items.CurrentChanging += new CurrentChangingEventHandler(Items_CurrentChanging);
            playerOriginal.Title = "Original";
            playerOriginal.WindowState = WindowState.Maximized;
            playerChanges.Title = "Preview Changes";
            playerChanges.WindowState = WindowState.Maximized;
            playerChanges.Player.Player.PositionChanged += Player_PositionChanged;
            playerMpc.SetPath();
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
            MediaEncodingCompletedWindow.Instance(e);
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ClosePreview();
            await business.DeletePreviewFilesAsync();
            if (callback != null)
                callback();
        }

        private void Window_Activated(object sender, EventArgs e) {
            if (playerChanges.Player.Position > 0)
                encodeSettings.Position = playerChanges.Player.Position;
        }

        private async void SelectVideoButton_Click(object sender, RoutedEventArgs e) {
            VideoListItem Result = SearchVideoWindow.Instance(new SearchSettings() {
                MediaType = MediaType.Video,
                ConditionField = FieldConditionEnum.FileExists,
                ConditionValue = BoolConditionEnum.Yes,
                RatingCategory = "Height",
                RatingOperator = OperatorConditionEnum.Smaller
            }, false);
            if (Result != null && Result.FileName != null) {
                ClosePreview();
                encodeSettings.AutoCalculateSize = false;
                encodeSettings.FileName = null;
                encodeSettings.CustomScript = null;
                SettingsTab.SelectedIndex = 0;
                encodeSettings.FileName = Result.FileName;
                encodeSettings.CropBottom = 0;
                encodeSettings.CropLeft = 0;
                encodeSettings.CropRight = 0;
                encodeSettings.CropTop = 0;

                try {
                    await business.PreparePreviewFile(encodeSettings, true);
                } catch (Exception ex) {
                    MessageBox.Show(this, ex.Message, "Cannot Open File", MessageBoxButton.OK, MessageBoxImage.Error);
                    encodeSettings.FileName = "";
                }
                encodeSettings.AutoCalculateSize = true;
            }
        }

        public void SetEncodeSettings(MediaEncoderSettings value) {
            isBinding = true;
            SettingsTab.SelectedIndex = (String.IsNullOrEmpty(value.CustomScript) ? 0 : 2);
            encodeSettings = value;
            this.DataContext = value;
            isBinding = false;
        }

        public void ClosePreview() {
            if (playerOriginal.Visibility == Visibility.Visible)
                playerOriginal.Close();
            if (playerChanges.Visibility == Visibility.Visible)
                playerChanges.Close();
            playerMpc.Close();
        }

        private async void PreviewOriginalButton_Click(object sender, RoutedEventArgs e) {
            await PlayVideoAsync(playerOriginal, Settings.NaturalGroundingFolder + encodeSettings.FileName);
        }

        private async void PreviewChangesButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                business.GenerateScript(encodeSettings, true, false);
                await PlayVideoAsync(playerChanges, Settings.TempFilesPath + "Preview.avs");
            }
        }

        private void PreviewMpcButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                business.GenerateScript(encodeSettings, false, true);
                // await playerMpc.PlayVideoAsync(Settings.TempFilesPath + "Preview.avs");
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
                ClosePreview();
                SetEncodeSettings((MediaEncoderSettings)encodeSettings.Clone());
                encodeSettings.FileName = "";
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
            bool Error = !IsValid(this) ||
                string.IsNullOrEmpty(encodeSettings.FileName) ||
                !encodeSettings.SourceHeight.HasValue ||
                !encodeSettings.SourceWidth.HasValue ||
                !encodeSettings.SourceFrameRate.HasValue;
            if (Error)
                MessageBox.Show(this, "You must enter required file information.", "Validation Error");
            return !Error;
        }

        private bool IsValid(DependencyObject obj) {
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
                LogicalTreeHelper.GetChildren(obj)
                .OfType<DependencyObject>()
                .All(IsValid);
        }


        private async void ConvertToAviCheckbox_Click(object sender, RoutedEventArgs e) {
            if (isBinding)
                return;
            ConvertToAviCheckbox.IsEnabled = false;
            await business.PreparePreviewFile(encodeSettings, false);
            ConvertToAviCheckbox.IsEnabled = true;
        }

        /// <summary>
        /// Generates script when going to Script tab, and prevents returning to Transform tab without a confirmation to lose changes.
        /// </summary>
        private void Items_CurrentChanging(object sender, CurrentChangingEventArgs e) {
            if (string.IsNullOrEmpty(encodeSettings.FileName))
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

        private void AudioQuality_TextChanged(object sender, TextChangedEventArgs e) {
            AudioBitrateLabel.Content = string.Format("~{0} kbps", business.ConvertAudioQualityToBitrate(encodeSettings.AudioQuality));
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
            encodeSettings.EncodeQuality = 24;
            encodeSettings.EncodePreset = EncodePresets.veryslow;
        }

        private void Codec265Option_Click(object sender, RoutedEventArgs e) {
            encodeSettings.EncodeQuality = 23;
            encodeSettings.EncodePreset = EncodePresets.medium;
        }
    }
}
