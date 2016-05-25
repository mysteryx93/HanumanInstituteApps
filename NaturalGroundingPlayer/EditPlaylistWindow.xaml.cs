using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Business;
using DataAccess;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for EditPlaylistWindow.xaml
    /// </summary>
    public partial class EditPlaylistWindow : Window {
        public static void Instance(Action callback) {
            EditPlaylistWindow NewForm = new EditPlaylistWindow();
            NewForm.callback = callback;
            SessionCore.Instance.Windows.Show(NewForm);
        }

        private DispatcherTimer timerChangeFilters;
        private bool isLoaded;
        protected Action callback;
        private WindowHelper helper;
        private bool isLoadingMediaInfo;

        public EditPlaylistWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            timerChangeFilters = new DispatcherTimer();
            timerChangeFilters.Interval = TimeSpan.FromSeconds(1);
            timerChangeFilters.Tick += timerChangeFilters_Tick;
            this.DataContext = MediaList.Settings;

            RatingCategoryCombo.ItemsSource = await MediaList.business.GetRatingCategoriesAsync(true);
            // Load categories list.
            await MediaList.LoadDataAsync();
            // Auto-attach files.
            await MediaList.ManualLoadPlaylist();
            await LoadMediaInfoAsync();
            isLoaded = true;
        }

        private async Task LoadMediaInfoAsync() {
            Progress<int> progress = new Progress<int>();
            int TotalCount = -1;
            progress.ProgressChanged += (sender, e) => {
                // First progress report is total count.
                if (TotalCount == -1)
                    TotalCount = e;
                else if (e < TotalCount)
                    // Subsequent reports contains quantity of files done.
                    StatusText.Text = string.Format("Updating files: {0} / {1}", e, TotalCount);
                else {
                    isLoadingMediaInfo = false;
                    DisplayVideoCount();
                }
            };
            isLoadingMediaInfo = true;
            bool HasChanges = await MediaList.business.LoadMediaInfoAsync(progress);
            isLoadingMediaInfo = false;
            DisplayVideoCount();
            //if (HasChanges) {
            //    await MediaList.LoadDataAsync();
            //}
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

        private void MediaList_DataLoaded(object sender, EventArgs e) {
            DisplayVideoCount();
        }

        private void DisplayVideoCount() {
            if (!isLoadingMediaInfo)
                StatusText.Text = string.Format("{0} {1}{2} found",
                    MediaList.PlayListCount,
                    MediaList.Settings.MediaType.ToString().ToLower(),
                    MediaList.PlayListCount > 1 ? "s" : "");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            MediaList.EditSelection();
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            callback?.Invoke();
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
            MediaList.EditSelection();
        }
    }
}
