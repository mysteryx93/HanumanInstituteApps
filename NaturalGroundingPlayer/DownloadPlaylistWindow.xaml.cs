using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for DownloadPlaylist.xaml
    /// </summary>
    public partial class DownloadPlaylistWindow : Window {
        public static void Instance(Action callback) {
            DownloadPlaylistWindow NewForm = new DownloadPlaylistWindow();
            NewForm.callback = callback;
            SessionCore.Instance.Windows.Show(NewForm);
        }

        private DownloadPlaylistBusiness business = new DownloadPlaylistBusiness();
        private DispatcherTimer timerChangeFilters;
        private bool isLoaded = false;
        private WindowHelper helper;
        protected Action callback;
        CancellationTokenSource ScanCancel;


        public DownloadPlaylistWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            business.DownloadManager = SessionCore.Instance.Business.DownloadManager;
            timerChangeFilters = new DispatcherTimer();
            timerChangeFilters.Interval = TimeSpan.FromSeconds(1);
            timerChangeFilters.Tick += timerChangeFilters_Tick;

            this.DataContext = MediaList.Settings;
            MediaList.Settings.SetCondition(FieldConditionEnum.HasDownloadUrl, true);
            RatingCategoryCombo.ItemsSource = await MediaList.business.GetRatingCategoriesAsync(true);
            await MediaList.LoadDataAsync();
            isLoaded = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (ScanCancel != null)
                ScanCancel.Cancel();
            if (callback != null)
                callback();
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

        private void SelectAllButton_Click(object sender, RoutedEventArgs e) {
            MediaList.VideosView.SelectAll();
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e) {
            MediaList.VideosView.SelectedItem = null;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e) {
            if (ScanCancel == null) {
                ScanButton.Content = "_Cancel";
                ScanCancel = new CancellationTokenSource();
                await business.StartScan(SelectedItems, ScanCancel.Token);
            } else
                ScanCancel.Cancel();
            ScanCancel = null;
            ScanButton.Content = "_Scan";
            DownloadButton.IsEnabled = true;
            UpgradeButton.IsEnabled = true;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e) {
            DownloadButton.IsEnabled = false;
            await business.StartDownload(SelectedItems, false);
            DownloadButton.IsEnabled = true;
        }

        private async void UpgradeButton_Click(object sender, RoutedEventArgs e) {
            UpgradeButton.IsEnabled = false;
            await business.StartDownload(SelectedItems, true);
            UpgradeButton.IsEnabled = true;
        }

        private List<VideoListItem> SelectedItems {
            get {
                return ((System.Collections.IList)MediaList.VideosView.SelectedItems).Cast<VideoListItem>().ToList();
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

        private void MediaList_DataLoaded(object sender, EventArgs e) {
            business.LoadStatusFromCache(MediaList.business.Playlist);
        }
    }
}
