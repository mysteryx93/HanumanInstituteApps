using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for ManualPlayerWindow.xaml
    /// </summary>
    public partial class ManualPlayerWindow : Window, ILayerContainer {
        public static void Instance(MainWindow main) {
            main.DetachControls();
            ManualPlayerWindow NewForm = new ManualPlayerWindow();
            NewForm.mainForm = main;
            SessionCore.Instance.Windows.Show(NewForm);
        }

        public ManualPlayerWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private WindowHelper helper;
        private MainWindow mainForm;
        private DispatcherTimer timerChangeFilters;
        private bool isLoaded = false;
        private bool isClosingManual = false;
        public SearchSettings mainSettings;

        private async void window_Loaded(object sender, RoutedEventArgs e) {
            this.Left = mainForm.Left;
            this.Top = mainForm.Top;
            this.Title = mainForm.Title + " [Manual Mode]";
            AttachControls();
            SessionCore.Instance.Business.NowPlaying += business_NowPlaying;
            SessionCore.Instance.Business.DisplayPlayTime += business_DisplayPlayTime;
            SessionCore.Instance.Business.Loop = false;

            timerChangeFilters = new DispatcherTimer();
            timerChangeFilters.Interval = TimeSpan.FromSeconds(1);
            timerChangeFilters.Tick += timerChangeFilters_Tick;
            // Keep main window's settings and it will be set back when closing manual mode.
            mainSettings = SessionCore.Instance.Business.FilterSettings;
            MediaList.Settings.RatingRatio = mainSettings.RatingRatio;
            MediaList.Settings.SetCondition(FieldConditionEnum.IsInDatabase, true);
            SessionCore.Instance.Business.FilterSettings = MediaList.Settings;
            this.DataContext = MediaList.Settings;

            RatingCategoryCombo.ItemsSource = await MediaList.business.GetRatingCategoriesAsync(false);
            await MediaList.LoadDataAsync();
            isLoaded = true;
        }

        private void Settings_Changed(object sender, RoutedEventArgs e) {
            if (isLoaded && !MediaList.IsLoading) {
                timerChangeFilters.Stop();
                timerChangeFilters.Start();
            }
        }

        private void RatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (this.IsLoaded) {
                SessionCore.Instance.RatingViewer.Ratio = (double)RatioSlider.Value;
                Settings_Changed(null, null);
            }
        }

        /// <summary>
        /// When exiting manual mode, this must be called instead of Close() otherwise the application will end.
        /// </summary>
        public void CloseManual() {
            isClosingManual = true;
            this.Close();
        }

        private async void window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (isClosingManual) {
                DetachControls();
                SessionCore.Instance.Business.NowPlaying -= business_NowPlaying;
                SessionCore.Instance.Business.DisplayPlayTime -= business_DisplayPlayTime;
                mainForm.AttachControls();
                mainForm.Left = this.Left;
                mainForm.Top = this.Top;
                SessionCore.Instance.Business.Loop = SessionCore.Instance.Menu.menuLoop.IsChecked;
                SessionCore.Instance.Business.ResetPlayerMode();
                mainSettings.RatingRatio = MediaList.Settings.RatingRatio;
                SessionCore.Instance.Business.FilterSettings = mainSettings;
                await SessionCore.Instance.Business.SelectNextVideoAsync(1);
                // mainForm.business_PlaylistChanged(null, null);
            } else
                Application.Current.Shutdown();
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e) {
            timerChangeFilters.Stop();
            timerChangeFilters.Start();
        }

        private void MediaList_SelectionChanged(object sender, EventArgs e) {
            if (MediaList.SelectedItem != null)
                PlayButton.Content = (MediaList.SelectedItem.FileExists ? "_Play" : "_Download");
        }

        private void MediaList_ItemDoubleClick(object sender, EventArgs e) {
            PlayButton_Click(this, null);
        }

        private async void ShowAllFiles_Click(object sender, RoutedEventArgs e) {
            MediaList.Settings.ConditionValue = ShowAllFiles.IsChecked.Value ? BoolConditionEnum.None : BoolConditionEnum.Yes;
            if (IsLoaded && !MediaList.IsLoading) {
                await MediaList.LoadDataAsync();
                SearchText.Focus();
            }
        }

        private async void timerChangeFilters_Tick(object sender, EventArgs e) {
            if (!MediaList.IsLoading) {
                if (RatingValueText.IsFocused)
                    RatingValueText.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                timerChangeFilters.Stop();
                await MediaList.LoadDataAsync();
            }
            //else
            //    mustReload = true;
        }

        public void AttachControls() {
            MainMenuContainer.Content = SessionCore.Instance.Menu;
            RatingViewerContainer.Content = SessionCore.Instance.RatingViewer;
            LayersContainer.Content = SessionCore.Instance.Layers;
            InputBindings.AddRange(mainForm.InputBindings);
            CommandBindings.AddRange(mainForm.CommandBindings);
            SessionCore.Instance.Downloads.Owner = this;

            // Adjust menu
            foreach (MenuItem item in SessionCore.Instance.Menu.menuSession.Items.OfType<MenuItem>()) {
                if (item != SessionCore.Instance.Menu.menuStartSession) {
                    if (item == SessionCore.Instance.Menu.menuManualMode || item == SessionCore.Instance.Menu.menuIgnorePos) {
                        EditInputBinding(item, ModifierKeys.Control);
                    } else {
                        item.IsEnabled = false;
                        EditInputBinding(item, ModifierKeys.None);
                    }
                }
            }
            EditInputBinding(SessionCore.Instance.Menu.menuEditVideo, ModifierKeys.Control);
            EditInputBinding(SessionCore.Instance.Menu.menuDownloadVideo, ModifierKeys.Control);
            EditInputBinding(SessionCore.Instance.Menu.menuSettings, ModifierKeys.Control);

            LayersRow.Height = new GridLength(SessionCore.Instance.Layers.Height);
            if (SessionCore.Instance.Business.CurrentVideo != null) {
                business_NowPlaying(null, new NowPlayingEventArgs());
            }
        }

        public void DetachControls() {
            MainMenuContainer.Content = null;
            RatingViewerContainer.Content = null;
            LayersContainer.Content = null;
            SessionCore.Instance.Downloads.Owner = mainForm;

            // Reset menu display.
            foreach (MenuItem item in SessionCore.Instance.Menu.menuSession.Items.OfType<MenuItem>()) {
                item.IsEnabled = true;
            }
            ResetInputBinding(SessionCore.Instance.Menu.menuEditVideo, Key.E);
            ResetInputBinding(SessionCore.Instance.Menu.menuIgnorePos, Key.I);
            ResetInputBinding(SessionCore.Instance.Menu.menuManualMode, Key.M);
            ResetInputBinding(SessionCore.Instance.Menu.menuDownloadVideo, Key.D);
            ResetInputBinding(SessionCore.Instance.Menu.menuSettings, Key.S);
            SessionCore.Instance.Menu.menuManualMode.IsChecked = false;

            mainForm.ResetHeight();
            mainForm.AdjustHeight(SessionCore.Instance.Layers.Height);
        }

        private void EditInputBinding(MenuItem menu, ModifierKeys modifier) {
            KeyConverter k = new KeyConverter();
            Key key = (Key)k.ConvertFromString(menu.InputGestureText);
            InputBindingCollection InputBindingsCopy = new InputBindingCollection(InputBindings);
            foreach (KeyBinding item in InputBindingsCopy) {
                if (item.Key == key) {
                    if (modifier == ModifierKeys.None)
                        InputBindings.Remove(item);
                    else {
                        item.Modifiers = modifier;
                        if (modifier == ModifierKeys.Control)
                            menu.InputGestureText = "CTRL+" + menu.InputGestureText;
                    }
                    break;
                }
            }
        }

        private void ResetInputBinding(MenuItem menu, Key key) {
            KeyConverter k = new KeyConverter();
            menu.InputGestureText = k.ConvertToString(key);
            foreach (KeyBinding item in InputBindings) {
                if (item.Key == key) {
                    item.Modifiers = ModifierKeys.None;
                    break;
                }
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e) {
            VideoListItem VideoInfo = MediaList.SelectedItem;
            if (VideoInfo != null) {
                if (VideoInfo.FileName != null) {
                    // File exists, play.
                    if (SessionCore.Instance.Business.IsStarted) {
                        if (SessionCore.Instance.Business.IsPaused)
                            SessionCore.Instance.Menu.ResumeSession();
                        await SessionCore.Instance.Business.SetNextVideoFileAsync(PlayerMode.Manual, VideoInfo.FileName);
                        if (PlayNextCheck.IsChecked == false)
                            await SessionCore.Instance.Business.SkipVideoAsync();
                    } else
                        await SessionCore.Instance.Menu.CommandBinding_StartSessionAsync(VideoInfo.FileName);
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

        private void business_NowPlaying(object sender, NowPlayingEventArgs e) {
            Media VideoInfo = SessionCore.Instance.Business.CurrentVideo;
            txtCurrentVideo.Text = SessionCore.Instance.Business.GetVideoDisplayTitle(VideoInfo);
            bool IsPrefEdit = SessionCore.Instance.RatingViewer.UpdatePreference();
            if (e.ReloadInfo)
                MediaList.EditForm_Closed(VideoInfo);
            else if (IsPrefEdit)
                MediaList.EditForm_Closed(SessionCore.Instance.RatingViewer.Video);
            SessionCore.Instance.RatingViewer.Video = VideoInfo;
        }

        private void business_DisplayPlayTime(object sender, EventArgs e) {
            txtSessionTime.Text = FormatHelper.FormatTimeSpan(SessionCore.Instance.Business.SessionTotalSeconds);
            if (SessionCore.Instance.Business.IsPlaying == false)
                txtSessionTime.Text += " (Paused)";
        }

        public void AdjustHeight(double height) {
            using (var d = Dispatcher.DisableProcessing()) {
                Height += height * Settings.Zoom;
                LayersRow.Height = new GridLength(LayersRow.Height.Value + height);
            }
        }
    }
}
