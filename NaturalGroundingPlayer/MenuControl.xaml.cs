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
using System.Windows.Navigation;
using Business;
using DataAccess;
using System.Diagnostics;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MenuControl.xaml
    /// </summary>
    public partial class MenuControl : UserControl {
        public MenuControl() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) {
                if (SessionCore.Instance.Downloads != null) {
                    SessionCore.Instance.Downloads.IsVisibleChanged += Downloads_IsVisibleChanged;
                }

                // Move keyboard shortcuts to the form.                
                SessionCore.Instance.Windows.Current.InputBindings.AddRange(this.InputBindings);
                SessionCore.Instance.Windows.Current.CommandBindings.AddRange(this.CommandBindings);
                this.InputBindings.Clear();
                this.CommandBindings.Clear();
            }
        }

        private async void CommandBinding_StartSession(object sender, ExecutedRoutedEventArgs e) {
            if (!menuStartSession.IsEnabled)
                return;

            if (!SessionCore.Instance.Business.IsStarted)
                await CommandBinding_StartSessionAsync(null);
            else if (SessionCore.Instance.Business.IsPaused)
                ResumeSession();
            else
                PauseSession();
        }

        public async Task CommandBinding_StartSessionAsync(string fileName) {
            menuStartSession.Header = "Pause _Session";
            menuEditVideo.IsEnabled = true;

            foreach (MenuItem item in menuSession.Items.OfType<MenuItem>()) {
                item.IsEnabled = true;
            }

            await SessionCore.Instance.Business.RunPlayerAsync(fileName);
        }

        public void PauseSession() {
            if (SessionCore.Instance.Business.IsStarted && !SessionCore.Instance.Business.IsPaused) {
                SessionCore.Instance.Business.PauseSession();
                menuStartSession.Header = "Resume _Session";
            }
        }

        public void ResumeSession() {
            if (SessionCore.Instance.Business.IsStarted && SessionCore.Instance.Business.IsPaused) {
                SessionCore.Instance.Business.ResumeSession();
                menuStartSession.Header = "Pause _Session";
            }
        }

        private void CommandBinding_EditVideo(object sender, ExecutedRoutedEventArgs e) {
            if (!menuEditVideo.IsEnabled)
                return;

            SessionCore.Instance.RatingViewer.UpdatePreference();
            if (SessionCore.Instance.Business.CurrentVideo != null) {
                EditVideoWindow.Instance(SessionCore.Instance.Business.CurrentVideo.MediaId, null, delegate(Media result) {
                    if (result != null)
                        SessionCore.Instance.Business.ReloadVideoInfo();
                });
            }
        }

        private void CommandBinding_EditPlaylist(object sender, ExecutedRoutedEventArgs e) {
            if (!menuEditPlaylist.IsEnabled)
                return;

            EditPlaylistWindow.Instance(null);
        }


        private void CommandBinding_DownloadPlaylist(object sender, ExecutedRoutedEventArgs e) {
            if (!menuDownloadPlaylist.IsEnabled)
                return;

            DownloadPlaylistWindow.Instance(null);
        }

        private void CommandBinding_MoveFiles(object sender, ExecutedRoutedEventArgs e) {
            if (!menuMoveFiles.IsEnabled)
                return;

            MoveFilesWindow.Instance(null);
        }

        private void CommandBinding_DistributionGraph(object sender, ExecutedRoutedEventArgs e) {
            if (!menuDistributionGraph.IsEnabled)
                return;

            DistributionGraphWindow.Instance();
        }

        private void CommandBinding_Export(object sender, ExecutedRoutedEventArgs e) {
            if (!menuExport.IsEnabled)
                return;

            ExportWindow.Instance();
        }

        private void CommandBinding_Import(object sender, ExecutedRoutedEventArgs e) {
            if (!menuImport.IsEnabled)
                return;

            ImportWindow.Instance();
        }

        private void CommandBinding_Exit(object sender, ExecutedRoutedEventArgs e) {
            SessionCore.Instance.Windows.Current.Close();
            Application.Current.Shutdown();
        }

        private async void CommandBinding_Skip(object sender, ExecutedRoutedEventArgs e) {
            if (!menuSkip.IsEnabled)
                return;

            await SessionCore.Instance.Business.SkipVideoAsync().ConfigureAwait(false);
        }

        private void CommandBinding_Loop(object sender, ExecutedRoutedEventArgs e) {
            if (!menuLoop.IsEnabled)
                return;

            menuLoop.IsChecked = !menuLoop.IsChecked;

            SessionCore.Instance.Business.Loop = menuLoop.IsChecked;
        }

        private async void CommandBinding_ReplayLast(object sender, ExecutedRoutedEventArgs e) {
            if (!menuReplayLast.IsEnabled)
                return;

            await SessionCore.Instance.Business.ReplayLastAsync().ConfigureAwait(false);
        }

        private async void CommandBinding_SpecialRequest(object sender, ExecutedRoutedEventArgs e) {
            if (!menuSpecialRequest.IsEnabled)
                return;

            SearchSettings SearchSettings = new SearchSettings() {
                MediaType = MediaType.Video,
                RatingRatio = SessionCore.Instance.NgMain.RatioSlider.Value
            };
            VideoListItem Result = SearchVideoWindow.Instance(SearchSettings);
            if (Result != null)
                await SessionCore.Instance.Business.SetNextVideoFileAsync(PlayerMode.SpecialRequest, Result.FileName).ConfigureAwait(false);
        }

        private async void CommandBinding_RequestCategory(object sender, ExecutedRoutedEventArgs e) {
            if (!menuRequestCategory.IsEnabled)
                return;

            if (!menuRequestCategory.IsChecked) {
                SearchSettings Result = RequestCategoryWindow.Instance();
                if (Result != null)
                    await SetRequestCategoryModeAsync(Result);
            } else {
                await SetRequestCategoryModeAsync(null);
            }
        }

        private async Task SetRequestCategoryModeAsync(SearchSettings settings) {
            bool active = settings != null;
            menuRequestCategory.IsChecked = active;

            menuSpecialRequest.IsEnabled = !active;
            menuPause.IsEnabled = !active;
            menuPlayFire.IsEnabled = !active;
            menuManualMode.IsEnabled = !active;
            // IntensitySlider.IsEnabled = !menuPause.IsChecked;

            await SessionCore.Instance.Business.SetRequestCategoryAsync(settings);
        }

        public async void CommandBinding_Pause(object sender, ExecutedRoutedEventArgs e) {
            if (!menuPause.IsEnabled)
                return;

            menuPause.IsChecked = !menuPause.IsChecked;

            menuSpecialRequest.IsEnabled = !menuPause.IsChecked;
            menuPlayFire.IsEnabled = !menuPause.IsChecked;
            menuManualMode.IsEnabled = !menuPause.IsChecked;
            // IntensitySlider.IsEnabled = !menuPause.IsChecked;

            await SessionCore.Instance.Business.SetFunPauseAsync(menuPause.IsChecked);
        }

        public async void CommandBinding_PlayFire(object sender, ExecutedRoutedEventArgs e) {
            if (!menuPlayFire.IsEnabled)
                return;

            SearchSettings settings = new SearchSettings();
            settings.MediaType = MediaType.Video;
            settings.RatingRatio = SessionCore.Instance.NgMain.RatioSlider.Value;
            settings.RatingCategory = "Fire";
            settings.RatingOperator = OperatorConditionEnum.GreaterOrEqual;
            settings.RatingValue = 7.5f;
            settings.RatingFilters.Add(new SearchRatingSetting() {
                Category = "Length",
                Operator = OperatorConditionEnum.Smaller,
                Value = 12
            });

            VideoListItem Result = SearchVideoWindow.Instance(settings);
            if (Result != null)
                await SessionCore.Instance.Business.SetNextVideoFileAsync(PlayerMode.Fire, Result.FileName);
        }

        public void CommandBinding_IgnorePos(object sender, ExecutedRoutedEventArgs e) {
            if (!menuIgnorePos.IsEnabled)
                return;

            menuIgnorePos.IsChecked = !menuIgnorePos.IsChecked;

            SessionCore.Instance.Business.IgnorePos = menuIgnorePos.IsChecked;
        }

        public void CommandBinding_ManualMode(object sender, ExecutedRoutedEventArgs e) {
            if (!menuManualMode.IsEnabled)
                return;

            Window parent = Window.GetWindow(this);
            if (parent.GetType() == typeof(MainWindow)) {
                // Start manual mode.
                ManualPlayerWindow.Instance((MainWindow)parent);
                menuManualMode.IsChecked = true;
            } else if (parent.GetType() == typeof(ManualPlayerWindow)) {
                // Close manual mode.
                ((ManualPlayerWindow)parent).CloseManual();
                menuManualMode.IsChecked = false;
            }
        }

        public async void CommandBinding_AddAudio(object sender, ExecutedRoutedEventArgs e) {
            if (!menuAddAudio.IsEnabled)
                return;

            VideoListItem Result = SearchVideoWindow.Instance(new SearchSettings() { MediaType = MediaType.Audio });
            if (Result != null) {
                LayerAudioControl Layer = new LayerAudioControl();
                await Layer.OpenMediaAsync(SessionCore.Instance.Business.GetMediaObject(Result.FileName));
                SessionCore.Instance.Layers.Add(Layer);
            }
        }

        public void CommandBinding_AddImage(object sender, ExecutedRoutedEventArgs e) {
            if (!menuAddImage.IsEnabled)
                return;

            VideoListItem Result = SearchVideoWindow.Instance(new SearchSettings() { MediaType = MediaType.Image });
            if (Result != null) {
                LayerImageControl Layer = new LayerImageControl();
                Layer.OpenMedia(SessionCore.Instance.Business.GetMediaObject(Result.FileName));
                SessionCore.Instance.Layers.Add(Layer);
            }
        }

        public async void CommandBinding_AddVideo(object sender, ExecutedRoutedEventArgs e) {
            if (!menuAddVideo.IsEnabled)
                return;

            VideoListItem Result = SearchVideoWindow.Instance(new SearchSettings() { MediaType = MediaType.Video });
            if (Result != null) {
                LayerVideoControl Layer = new LayerVideoControl();
                await Layer.OpenMediaAsync(SessionCore.Instance.Business.GetMediaObject(Result.FileName));
                SessionCore.Instance.Layers.Add(Layer);
            }
        }

        public void CommandBinding_AddLucidVideo(object sender, ExecutedRoutedEventArgs e) {
            if (!menuAddLucidVideo.IsEnabled)
                return;

            VideoListItem Result = SearchVideoWindow.Instance(new SearchSettings() { MediaType = MediaType.Video });
            if (Result != null) {
                LayerLucidVideoControl Layer = new LayerLucidVideoControl();
                Layer.OpenMedia(SessionCore.Instance.Business.GetMediaObject(Result.FileName));
                SessionCore.Instance.Layers.Add(Layer);
            }
        }

        public void CommandBinding_DownloadVideo(object sender, ExecutedRoutedEventArgs e) {
            AddDownload.Instance(null);
        }

        public void CommandBinding_ViewDownloads(object sender, ExecutedRoutedEventArgs e) {
            if (!menuViewDownloads.IsEnabled)
                return;

            menuViewDownloads.IsChecked = !menuViewDownloads.IsChecked;

            if (menuViewDownloads.IsChecked) {
                SessionCore.Instance.Downloads.ShowAndPosition(ToolboxPosition.Right);
            } else
                SessionCore.Instance.Downloads.Hide();
        }

        public void CommandBinding_SetupWizard(object sender, ExecutedRoutedEventArgs e) {
            if (!menuSetupWizard.IsEnabled)
                return;

            if (Settings.SavedFile.MediaPlayerApp == MediaPlayerApplication.Mpc) {
                PauseSession();
                SetupWizard.Instance(null);
            } else
                MessageBox.Show("To use this wizard, you must first set MPC-HC as your media player under Options.", "MPC-HC Setup Wizard");
        }

        public void CommandBinding_Settings(object sender, ExecutedRoutedEventArgs e) {
            if (!menuSettings.IsEnabled)
                return;

            SettingsWindow.Instance();
        }

        private void CommandBinding_ForceOfLife(object sender, ExecutedRoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://www.forceoflife.net/"));
        }

        private void CommandBinding_BuyResources(object sender, ExecutedRoutedEventArgs e) {
            if (!menuBuyResources.IsEnabled)
                return;

            BuyResourcesWindow.Instance(null);
        }

        private void CommandBinding_EnergyTuneUp(object sender, ExecutedRoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://www.spiritualselftransformation.com/energy-tune-up"));
        }

        private void CommandBinding_OfficialResources(object sender, ExecutedRoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("http://sexenergysuccess.com/natural-grounding-resources-official/"));
        }

        private void CommandBinding_AboutUs(object sender, ExecutedRoutedEventArgs e) {
            AboutWindow.Instance();
        }

        private void Downloads_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (!SessionCore.Instance.Downloads.IsVisible)
                menuViewDownloads.IsChecked = false;
        }
    }
}
