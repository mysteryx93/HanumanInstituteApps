using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using EmergenceGuardian.NaturalGroundingPlayer.Business;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;
using EmergenceGuardian.CommonWpf;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ILayerContainer {
        private WindowHelper helper;
        public double DefaultHeight;
        public SearchSettings settings;
        private bool IsHeatAscending = true;
        private double LastHeatValue;
        private double RatioOffset; 

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            DefaultHeight = this.Height;
            SessionCore.Instance.Start(this, Properties.Resources.AppIcon);
            helper = new WindowHelper(this);
        }

        #region UI Events

        /// <summary>
        /// Initialize the window when loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.Title += " " + SessionCore.GetVersionText();
            AttachBusinessEvents(SessionCore.Instance.Business);
            SplashWindow.Instance(this, Properties.Resources.AppIcon);
        }

        public async Task InitializationCompleted() {
            settings = SessionCore.Instance.Business.FilterSettings;
            PolarityFocus.ItemsSource = SessionCore.Instance.Business.GetFocusCategories();
            ElementList.ItemsSource = await SessionCore.Instance.Business.GetElementCategoriesAsync();
            this.DataContext = settings;
            PolarityFocus.SelectedIndex = 1;
            Settings_Changed(null, null);
        }

        public void DetachControls() {
            MainMenuContainer.Content = null;
            RatingViewerContainer.Content = null;
            LayersContainer.Content = null;
            DetachBusinessEvents(SessionCore.Instance.Business);
        }

        public void AttachControls() {
            MainMenuContainer.Content = SessionCore.Instance.Menu;
            RatingViewerContainer.Content = SessionCore.Instance.RatingViewer;
            LayersContainer.Content = SessionCore.Instance.Layers;
            AttachBusinessEvents(SessionCore.Instance.Business);
        }

        private void AttachBusinessEvents(PlayerBusiness business) {
            business.GetConditions += business_GetConditions;
            business.IncreaseConditions += business_IncreaseConditions;
            business.PlaylistChanged += business_PlaylistChanged;
            business.DisplayPlayTime += business_DisplayPlayTime;
            business.NowPlaying += business_NowPlaying;
        }

        private void DetachBusinessEvents(PlayerBusiness business) {
            business.GetConditions -= business_GetConditions;
            business.IncreaseConditions -= business_IncreaseConditions;
            business.PlaylistChanged -= business_PlaylistChanged;
            business.DisplayPlayTime -= business_DisplayPlayTime;
            business.NowPlaying -= business_NowPlaying;
        }

        /// <summary>
        /// Update the display of conditions and notify the business layer of changes.
        /// </summary>
        private void Settings_Changed(object sender, RoutedEventArgs e) {
            if (this.DataContext != null) {
                if (IntensitySlider.Value == IntensitySlider.Maximum && GrowthSlider.Value > 0)
                    GrowthSlider.Value = 0;
                if (IntensitySlider.Value == IntensitySlider.Minimum && GrowthSlider.Value < 0)
                    GrowthSlider.Value = 0;
                ElementRangeSlider.IsEnabled = ElementList.SelectedIndex > 0;
                SessionCore.Instance.Business.IsMinimumIntensity = (IntensitySlider.Value == IntensitySlider.Minimum);
                if (IntensitySlider.Value > LastHeatValue)
                    IsHeatAscending = true;
                else if (IntensitySlider.Value < LastHeatValue)
                    IsHeatAscending = false;
                RatioSlider.Value = GetDefaultPriority(IsHeatAscending) + RatioOffset;
                LastHeatValue = IntensitySlider.Value;

                DisplayConditionsText();
                SessionCore.Instance.Business.ChangeConditions();
            }
        }

        #endregion

        #region Business Events

        private void business_NowPlaying(object sender, EventArgs e) {
            SessionCore.Instance.RatingViewer.Video = SessionCore.Instance.Business.CurrentVideo;
        }

        private void business_IncreaseConditions(object sender, EventArgs e) {
            IntensitySlider.Value += GrowthSlider.Value;
            // If decreasing heat, go down to Water from 1 above minimum
            if (GrowthSlider.Value < 0 && IntensitySlider.Value < IntensitySlider.Minimum + 1)
                IntensitySlider.Value = IntensitySlider.Minimum;
        }

        private async void business_GetConditions(object sender, GetConditionsEventArgs e) {
            var Filters = e.Conditions.RatingFilters;
            Filters.Clear();
            switch (SessionCore.Instance.Business.PlayMode) {
                case PlayerMode.Normal:
                    double MaxCond = IntensitySlider.Value + e.QueuePos * GrowthSlider.Value;
                    // If decreasing heat, go down to Water from 1 above minimum
                    if (GrowthSlider.Value < 0 && MaxCond < IntensitySlider.Minimum + 1)
                        MaxCond = IntensitySlider.Minimum;
                    if (MaxCond > IntensitySlider.Maximum)
                        MaxCond = IntensitySlider.Maximum;
                    if (MaxCond > IntensitySlider.Minimum) {
                        if (PolarityFocus.Text != "") {

                            // Exclude videos with Water >= 6
                            Filters.AddRange(FilterPresets.PresetWater(true));
                            // Exclude videos with Fire >= 8.5 && Intensity >= 9.5 unless we're at maximum heat.
                            if (MaxCond < IntensitySlider.Maximum)
                                Filters.AddRange(FilterPresets.PresetFire(true));

                            if (PolarityFocus.Text == "Intensity")
                                MaxCond -= 1;
                            // Min/Max Values.
                            SearchRatingSetting MinFilter = new SearchRatingSetting(PolarityFocus.Text, OperatorConditionEnum.GreaterOrEqual, Math.Round(MaxCond - ToleranceSlider.Value, 1));
                            SearchRatingSetting MaxFilter = new SearchRatingSetting(PolarityFocus.Text, OperatorConditionEnum.Smaller, Math.Round(MaxCond, 1));
                            if (PolarityFocus.Text == "Intensity" && MaxCond == IntensitySlider.Maximum - 1)
                                MaxFilter.Value = IntensitySlider.Maximum;
                            if (e.IncreaseTolerance) {
                                MinFilter.Value -= .5f;
                                MaxFilter.Value += .5f;
                            }
                            Filters.Add(MinFilter);
                            if (MaxFilter.Value < IntensitySlider.Maximum) // When at max heat, don't limit greater values
                                Filters.Add(MaxFilter);
                            if (PolarityFocus.Text == "Physical" || PolarityFocus.Text == "Emotional" || PolarityFocus.Text == "Spiritual") {
                                // Don't get videos that are more than .5 stronger on other values.
                                Filters.Add(new SearchRatingSetting("!" + PolarityFocus.Text, OperatorConditionEnum.Smaller, MaxFilter.Value + .5f));
                            } else if (PolarityFocus.Text == "Intensity") {
                                Filters.Add(new SearchRatingSetting("!Intensity", OperatorConditionEnum.Smaller, MaxFilter.Value + 2f));
                            }
                        }

                        // Secondary filter by element
                        if (ElementList.SelectedIndex > 0) {
                            Filters.Add(new SearchRatingSetting(ElementList.Text, OperatorConditionEnum.GreaterOrEqual, Math.Round(ElementRangeSlider.LowerValue, 0)));
                            double ElementMax = Math.Round(ElementRangeSlider.HigherValue, 0);
                            if (ElementMax < ElementRangeSlider.Maximum)
                                Filters.Add(new SearchRatingSetting(ElementList.Text, OperatorConditionEnum.Smaller, ElementMax + .1));
                        }
                    } else { // Water
                        IntensitySlider.Value = IntensitySlider.Minimum;
                        await SessionCore.Instance.Business.SetWaterVideosAsync(true);
                    }
                    // Change condition display after "Fire" returns to normal.
                    DisplayConditionsText();
                    break;
                case PlayerMode.WarmPause:
                case PlayerMode.Fire:
                    Filters.AddRange(FilterPresets.PresetPause());
                    break;
                case PlayerMode.Water:
                    if (!e.IncreaseTolerance) {
                        Filters.Add(new SearchRatingSetting("Water", OperatorConditionEnum.GreaterOrEqual, 4f));
                    } else {
                        Filters.Add(new SearchRatingSetting("Intensity", OperatorConditionEnum.Smaller, 7f));
                        Filters.Add(new SearchRatingSetting("Party", OperatorConditionEnum.Smaller, 2f));
                        Filters.Add(new SearchRatingSetting("Vitality", OperatorConditionEnum.Smaller, 2f));
                        Filters.Add(new SearchRatingSetting("Fire", OperatorConditionEnum.Smaller, 2f));
                        Filters.Add(new SearchRatingSetting("!", OperatorConditionEnum.Smaller, 9f));
                    }
                    break;
                case PlayerMode.RequestCategory:
                    // Filters are already defined in Business.FilterSettings.
                    Filters.Add(new SearchRatingSetting());
                    break;
            }
        }

        public void business_PlaylistChanged(object sender, EventArgs e) {
            txtVideosFound.Text = SessionCore.Instance.Business.LastSearchResultCount.ToString() + (SessionCore.Instance.Business.LastSearchResultCount > 1 ? " videos found" : " video found");
            NextVideosCombo.ItemsSource = SessionCore.Instance.Business.NextVideoOptions;
            if (NextVideosCombo.Items.Count > 0)
                NextVideosCombo.SelectedIndex = 0;
            txtCurrentVideo.Text = SessionCore.Instance.Business.CurrentVideo?.DisplayTitle;
            DisplayConditionsText();
        }

        private void business_DisplayPlayTime(object sender, EventArgs e) {
            txtSessionTime.Text = FormatHelper.FormatTimeSpan(SessionCore.Instance.Business.SessionTotalSeconds);
            if (SessionCore.Instance.Business.IsPlaying == false)
                txtSessionTime.Text += " (Paused)";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the conditions text.
        /// </summary>
        private void DisplayConditionsText() {
            string SelectedPolarity = ((RatingCategory)PolarityFocus.SelectedItem).Name;
            if (SessionCore.Instance.Business.IsMinimumIntensity && !SessionCore.Instance.Business.IsSpecialMode())
                IntensitySliderValue.Text = "Water...";
            else {
                switch (SessionCore.Instance.Business.PlayMode) {
                    case PlayerMode.Normal:
                    case PlayerMode.SpecialRequest:
                    case PlayerMode.Water:
                        double MaxCond = IntensitySlider.Value;
                        bool IsMaxIntensity = (MaxCond == IntensitySlider.Maximum);
                        if (SelectedPolarity == "Intensity")
                            MaxCond -= 1;
                        IntensitySliderValue.Text = SelectedPolarity != "" ? string.Format("{0:0.0} - {1:0.0}{2}", MaxCond - ToleranceSlider.Value, IsMaxIntensity ? IntensitySlider.Maximum : MaxCond, IsMaxIntensity ? "+" : "") : "";
                        IntensitySlider.IsEnabled = SelectedPolarity != "";
                        GrowthSlider.IsEnabled = SelectedPolarity != "";
                        ToleranceSlider.IsEnabled = SelectedPolarity != "";
                        break;
                    case PlayerMode.WarmPause:
                        IntensitySliderValue.Text = "Warm Pause";
                        break;
                    case PlayerMode.RequestCategory:
                        StringBuilder Result = new StringBuilder();
                        if (!string.IsNullOrEmpty(settings.Search))
                            Result.Append(settings.Search);
                        if (!string.IsNullOrEmpty(settings.RatingCategory) && settings.RatingValue.HasValue) {
                            if (Result.Length > 0)
                                Result.Append(", ");
                            Result.Append(settings.RatingCategory);
                            if (settings.RatingOperator == OperatorConditionEnum.GreaterOrEqual)
                                Result.Append(" >= ");
                            else if (settings.RatingOperator == OperatorConditionEnum.Equal)
                                Result.Append(" = ");
                            else if (settings.RatingOperator == OperatorConditionEnum.Smaller)
                                Result.Append(" < ");
                            Result.Append(settings.RatingValue.Value.ToString("0.0"));
                        }
                        IntensitySliderValue.Text = Result.ToString();
                        break;
                    case PlayerMode.Fire:
                        IntensitySliderValue.Text = "Fire!!";
                        break;
                }
            }

            // Sometimes the intensity slider doesn't get redrawn properly; force it.
            IntensitySlider.InvalidateVisual();
        }

        public void AdjustHeight(double height) {
            using (var d = Dispatcher.DisableProcessing()) {
                Height += height * Settings.Zoom;
                LayersRow.Height = new GridLength(LayersRow.Height.Value + height);
            }
        }

        public void ResetHeight() {
            using (var d = Dispatcher.DisableProcessing()) {
                Height = DefaultHeight * Settings.Zoom;
                LayersRow.Height = new GridLength(0);
            }
        }

        #endregion

        private void RatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (this.IsLoaded) {
                RatioOffset = RatioSlider.Value - GetDefaultPriority(IsHeatAscending);
                SessionCore.Instance.RatingViewer.Ratio = (double)RatioSlider.Value;
                Settings_Changed(null, null);
            }
        }

        private void window_Closed(object sender, EventArgs e) {
            PathManager.ClearTempFolder();
        }

        private async void SkipVideo_Click(object sender, RoutedEventArgs e) {
            if (SessionCore.Instance.Business.IsStarted)
                await SessionCore.Instance.Business.SkipVideoAsync().ConfigureAwait(false);
        }

        private async void SkipNextVideo_Click(object sender, RoutedEventArgs e) {
            if (SessionCore.Instance.Business.IsStarted)
                await SessionCore.Instance.Business.SelectNextVideoAsync(1, false).ConfigureAwait(false);
        }

        private void ElementRangeSlider_ValueChanged(object sender, RoutedEventArgs e) {
            ElementRangeText.Text = string.Format("{0:0} - {1:0}", ElementRangeSlider.LowerValue, ElementRangeSlider.HigherValue);
            Settings_Changed(sender, e);
        }

        private async void NextVideosCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Media Item = NextVideosCombo.SelectedItem as Media;
            if (Item != null)
                await SessionCore.Instance.Business.SetNextVideoOptionAsync(Item).ConfigureAwait(false);
        }

        private double GetDefaultPriority(bool isHeatAscending) {
            const double Deviation = .5; // Starts at 50% towards Height
            double SliderValue = (IntensitySlider.Value - IntensitySlider.Minimum) / (IntensitySlider.Maximum - IntensitySlider.Minimum);
            return (1 - SliderValue) * Deviation * (isHeatAscending ? -1 : 1);
        }
    }
}
