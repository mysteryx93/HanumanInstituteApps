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
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.Diagnostics;
using Business;
using DataAccess;
using System.Windows.Threading;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for BuyResourcesWindow.xaml
    /// </summary>
    public partial class BuyResourcesWindow : Window {
        public static void Instance(Action callback) {
            BuyResourcesWindow NewForm = new BuyResourcesWindow();
            NewForm.callback = callback;
            SessionCore.Instance.Windows.Show(NewForm);
        }
        
        private EditPlaylistBusiness business = new EditPlaylistBusiness();
        private GridViewColumn lastHeaderClicked;
        private ListSortDirection lastDirection = ListSortDirection.Ascending;
        protected Action callback;
        private WindowHelper helper;
        private bool isLoading = true;
        private DispatcherTimer timerChangeFilters;
        public SearchSettings settings = new SearchSettings();

        public BuyResourcesWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            timerChangeFilters = new DispatcherTimer();
            timerChangeFilters.Interval = TimeSpan.FromSeconds(1);
            timerChangeFilters.Tick += timerChangeFilters_Tick;
            lastHeaderClicked = ((GridView)VideosView.View).Columns[0];
            this.DataContext = settings;
            
            RatingCategoryCombo.ItemsSource = await business.GetRatingCategoriesAsync(true);
            await LoadDataAsync();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (callback != null)
                callback();
        }

        public async Task LoadDataAsync() {
            isLoading = true;
            if (lastHeaderClicked.DisplayMemberBinding != null)
                settings.OrderBy = ((Binding)lastHeaderClicked.DisplayMemberBinding).Path.Path;
            else
                settings.OrderBy = lastHeaderClicked.HeaderStringFormat;
            settings.OrderByDirection = lastDirection;
            await business.LoadBuyListAsync(settings, (BuyProductType)ProductsTab.SelectedIndex, !ShowMissing.IsChecked.Value);
            DisplayData();
            isLoading = false;
        }

        private void DisplayData() {
            int CurrentSelection = VideosView.SelectedIndex;
            VideosView.ItemsSource = business.Playlist;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(VideosView.ItemsSource);
            // view.GroupDescriptions.Add(new PropertyGroupDescription("BuyUrl"));
            view.GroupDescriptions.Add(new PropertyGroupDescription("Album"));

            // Display custom column header when custom rating is selected.
            if (string.IsNullOrEmpty(settings.RatingCategory) || business.Playlist.Where(v => v.Custom != null).Any() == false)
                CustomColumn.Header = "";
            else
                CustomColumn.Header = settings.RatingCategory.Substring(0, 3);

            // Maintain current selection.
            if (CurrentSelection < 0)
                VideosView.SelectedIndex = 0;
            else if (CurrentSelection < VideosView.Items.Count)
                VideosView.SelectedIndex = CurrentSelection;
            else
                VideosView.SelectedIndex = VideosView.Items.Count - 1;
        }

        private async void VideosView_ColumnHeaderClick(object sender, RoutedEventArgs e) {
            GridViewColumnHeader headerClicked =
                  e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null) {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding) {
                    if (headerClicked.Column != lastHeaderClicked) {
                        direction = ListSortDirection.Ascending;
                    } else {
                        if (lastDirection == ListSortDirection.Ascending) {
                            direction = ListSortDirection.Descending;
                        } else {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    lastHeaderClicked = headerClicked.Column;
                    lastDirection = direction;

                    await LoadDataAsync();
                }
            }
        }

        private void VideosView_ItemDoubleClick(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed)
                EditButton_Click(null, null);
        }

        private void VideosView_ItemRightButtonUp(object sender, MouseButtonEventArgs e) {
            if (VideosView.SelectedItem != null) {
                VideoListItem Item = (VideoListItem)VideosView.SelectedItem;
                ShowEditFormPopup(Item, sender as UIElement);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            if (VideosView.SelectedItem != null) {
                VideoListItem Item = (VideoListItem)VideosView.SelectedItem;
                ShowEditForm(Item);
            }
        }

        private void ShowEditForm(VideoListItem item) {
            EditVideoWindow.Instance(item.MediaId, item.FileName, EditForm_Closed);
        }


        private void ShowEditFormPopup(VideoListItem item, UIElement target) {
            EditVideoWindow.InstancePopup(target, PlacementMode.Mouse, item.MediaId, item.FileName, EditForm_Closed);
        }

        private async void EditForm_Closed(Media result) {
            if (result != null) {
                VideoListItem OldItem = (VideoListItem)VideosView.SelectedItem;
                // business.RefreshPlaylist(result, OldItem != null ? OldItem.FileName : null);
                await LoadDataAsync();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private async void ProductsTab_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (IsLoaded)
                await LoadDataAsync();
        }

        private void SearchText_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down && VideosView.SelectedIndex < VideosView.Items.Count)
                VideosView.SelectedIndex += 1;
            if (e.Key == Key.Up && VideosView.SelectedIndex > 0)
                VideosView.SelectedIndex -= 1;
            VideosView.ScrollIntoView(VideosView.SelectedItem);
        }

        private void Settings_Changed(object sender, RoutedEventArgs e) {
            if (!isLoading) {
                timerChangeFilters.Stop();
                timerChangeFilters.Start();
            }
        }

        private async void timerChangeFilters_Tick(object sender, EventArgs e) {
            if (!isLoading) {
                if (RatingValueText.IsFocused)
                    RatingValueText.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                timerChangeFilters.Stop();
                await LoadDataAsync();
            }
        }

        private async void ShowMissing_Click(object sender, RoutedEventArgs e) {
            if (IsLoaded) {
                await LoadDataAsync();
                SearchText.Focus();
            }
        }
    }
}
