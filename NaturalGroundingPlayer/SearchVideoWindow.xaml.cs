using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Business;
using DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for FindVideoWindow.xaml
    /// </summary>
    public partial class SearchVideoWindow : Window {
        public static VideoListItem Instance(SearchSettings settings, bool showAllFilesVisible) {
            SearchVideoWindow NewForm = new SearchVideoWindow();
            NewForm.settings = settings;
            NewForm.showAllFilesVisible = showAllFilesVisible;
            SessionCore.Instance.Windows.ShowDialog(NewForm);
            return NewForm.selection;
        }

        private DispatcherTimer timerChangeFilters;
        private bool isLoaded;
        protected VideoListItem selection;
        private WindowHelper helper;
        public SearchSettings settings;
        public bool showAllFilesVisible;

        public SearchVideoWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            timerChangeFilters = new DispatcherTimer();
            timerChangeFilters.Interval = TimeSpan.FromSeconds(1);
            timerChangeFilters.Tick += timerChangeFilters_Tick;

            MediaList.Settings = settings;

            if (showAllFilesVisible)
                settings.ConditionFilters.Add(new SearchConditionSetting()); // This will be associated with ShowAllFiles option.
            else
                ShowAllFiles.Visibility = Visibility.Hidden;

            if (settings.ConditionField == FieldConditionEnum.None)
                settings.SetCondition(FieldConditionEnum.FileExists, BoolConditionEnum.Yes);

            if (settings.MediaType == MediaType.None)
                this.Title = "Search Files";
            else if (settings.MediaType == MediaType.Video)
                this.Title = "Search Videos";
            else if (settings.MediaType == MediaType.Audio)
                this.Title = "Search Audios";
            else if (settings.MediaType == MediaType.Image)
                this.Title = "Search Images";

            RatingCategoryCombo.ItemsSource = await MediaList.business.GetRatingCategoriesAsync(true);
            this.DataContext = settings;
            await MediaList.LoadDataAsync();
            isLoaded = true;
        }

        private void Settings_Changed(object sender, RoutedEventArgs e) {
            if (isLoaded && !MediaList.IsLoading) {
                timerChangeFilters.Stop();
                timerChangeFilters.Start();
            }
        }

        private async void timerChangeFilters_Tick(object sender, EventArgs e) {
            if (!MediaList.IsLoading) {
                if (RatingValueText.IsFocused)
                    RatingValueText.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                timerChangeFilters.Stop();
                await MediaList.LoadDataAsync();
            }
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e) {
            timerChangeFilters.Stop();
            timerChangeFilters.Start();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e) {
            selection = MediaList.SelectedItem;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void VideosView_ItemDoubleClick(object sender, MouseButtonEventArgs e) {
            SelectButton_Click(null, null);
        }

        private async void ShowAllFiles_Click(object sender, RoutedEventArgs e) {
            MediaList.Settings.SetCondition(FieldConditionEnum.IsInDatabase, ShowAllFiles.IsChecked.Value ? BoolConditionEnum.None : BoolConditionEnum.Yes);
            if (IsLoaded && !MediaList.IsLoading) {
                await MediaList.LoadDataAsync();
                SearchText.Focus();
            }
        }

        private void RatingCategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            bool IsIntVisible = RatingValueIntText.Visibility == Visibility.Visible;
            bool IsIntSelection = new string[] { "Length", "Height" }.Contains(RatingCategoryCombo.SelectedValue);

            if (IsIntSelection && !IsIntVisible) {
                RatingValueText.Visibility = Visibility.Hidden;
                RatingValueIntText.Visibility = Visibility.Visible;
                RatingValueIntText.Text = null;
            } else if (!IsIntSelection && IsIntVisible) {
                RatingValueText.Visibility = Visibility.Visible;
                RatingValueIntText.Visibility = Visibility.Hidden;
                RatingValueText.Text = null;
            }

            Settings_Changed(null, null);
        }

        private void MediaList_ItemDoubleClick(object sender, EventArgs e) {
            SelectButton_Click(this, null);
        }
    }
}
