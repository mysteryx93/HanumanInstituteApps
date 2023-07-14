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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Business;
using DataAccess;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MediaGrid.xaml
    /// </summary>
    public partial class MediaGrid : UserControl {
        private GridViewColumn lastHeaderClicked;
        private ListSortDirection lastDirection = ListSortDirection.Ascending;
        private List<VideoListItem> selectionList = new List<VideoListItem>();
        public EditPlaylistBusiness business = new EditPlaylistBusiness();
        public SearchSettings Settings { get; set; }
        public SearchFilterEnum SearchGroupType { get; set; } = SearchFilterEnum.Artist;
        public bool PopupEnabled { get; set; } = true;
        public bool IsPreferenceVisible { get; set; } = true;
        public bool IsIntensityVisible { get; set; } = true;
        public bool IsCustomVisible { get; set; } = true;
        public bool IsStatusVisible { get; set; } = false;
        public bool DisableLoadData { get; set; }
        public bool AllowMultiSelect { get; set; } = false;
        public bool IsLoading { get; private set; }
        public bool IsDetailView { get; set; }
        public UIElement FocusControl { get; set; }
        private bool isFormLoaded = false;
        public event EventHandler DataLoaded;
        public event EventHandler ItemDoubleClick;
        public event EventHandler SelectionChanged;
        private Storyboard StoryboardOpenDetail;
        private Storyboard StoryboardCloseDetail;

        public MediaGrid() {
            InitializeComponent();
            Settings = new SearchSettings();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e) {
            StoryboardOpenDetail = (Storyboard)FindResource("StoryboardOpenDetail");
            StoryboardCloseDetail = (Storyboard)FindResource("StoryboardCloseDetail");
            if (!IsPreferenceVisible)
                ListGridView.Columns.Remove(PreferenceColumn);
            if (!IsIntensityVisible)
                ListGridView.Columns.Remove(IntensityColumn);
            if (!IsCustomVisible)
                ListGridView.Columns.Remove(CustomColumn);
            if (!AllowMultiSelect)
                ListGridView.Columns.Remove(SelectionColumn);
            if (!IsStatusVisible)
                ListGridView.Columns.Remove(StatusColumn);
            VideosView.SelectionMode = AllowMultiSelect ? SelectionMode.Multiple : SelectionMode.Single;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            lastHeaderClicked = ((GridView)VideosView.View).Columns[0];
            Window.GetWindow(this).Closed += MediaGrid_Closed;

            // Bind to parent window
            Window.GetWindow(this).PreviewKeyDown += Window_PreviewKeyDown;
            // Bind to search box.
            var ParentBoxes = Window.GetWindow(this).FindVisualChildren<TextBox>();
            if (FocusControl == null && ParentBoxes.Count() > 0)
                FocusControl = ParentBoxes.First();
            // Bind PreviewKeyDown to every TextBox on the page.
            foreach (TextBox item in ParentBoxes) {
                item.PreviewKeyDown += SearchTextControl_PreviewKeyDown;
            }

            await LoadCategoriesAsync();

            isFormLoaded = true;
        }

        void MediaGrid_Closed(object sender, EventArgs e) {
        }

        public VideoListItem SelectedItem {
            get {
                if (IsDetailView)
                    return (VideoListItem)VideosView.SelectedItem;
                else
                    return null;
            }
        }

        public async Task LoadDataAsync() {
            if (DisableLoadData)
                return;

            // Searching automatically opens details view.
            //if (!string.IsNullOrEmpty(Settings.Search) && !IsDetailView) {
            //    CategoriesList.SelectedIndex = 0; // All
            //}

            // Make sure UserControl_Loaded is completed.
            while (!isFormLoaded) {
                await Task.Delay(50);
            }

            IsLoading = true;
            if (IsDetailView)
                await LoadDetailsAsync();
            else if (Settings.Search != "") {
                // Open search details on 'All'
                CategoriesList.SelectedIndex = 0;
                Settings.FilterValue = null;
                await LoadDetailsAsync();
                await ShowDetailsAsync();
            } else 
                await LoadCategoriesAsync();
            IsLoading = false;
        }

        public async Task LoadDetailsAsync() {
            if (lastHeaderClicked == StatusColumn)
                Settings.OrderBy = "Artist";
            else if (lastHeaderClicked.DisplayMemberBinding != null)
                Settings.OrderBy = ((Binding)lastHeaderClicked.DisplayMemberBinding).Path.Path;
            else
                Settings.OrderBy = lastHeaderClicked.HeaderStringFormat;
            Settings.OrderByDirection = lastDirection;
            await business.LoadPlaylistAsync(Settings, true);
            DataLoaded?.Invoke(this, null);
            DisplayData();
        }

        public async Task ManualLoadPlaylist() {
            await business.LoadPlaylistAsync(Settings, true);
        }

        public async Task LoadCategoriesAsync() {
            int CurrentSelection = CategoriesList.SelectedIndex;
            CategoriesList.ItemsSource = await business.LoadCategoriesAsync(Settings, SearchGroupType);

            // Find separator position.
            int Pos = 0;
            foreach (SearchCategoryItem item in CategoriesList.ItemsSource) {
                if (item.FilterType == SearchFilterEnum.None)
                    break;
                else
                    Pos++;
            }
            // First category item is right after separator.
            Pos++;

            // Maintain selection.
            if (CurrentSelection < Pos)
                CategoriesList.SelectedIndex = (CategoriesList.Items.Count > Pos) ? Pos : 0;
            else if (CurrentSelection < CategoriesList.Items.Count)
                CategoriesList.SelectedIndex = CurrentSelection;
            else
                CategoriesList.SelectedIndex = CategoriesList.Items.Count - 1;
        }

        public void Clear() {
            VideosView.ItemsSource = null;
        }

        public void DisplayData(List<VideoListItem> items) {
            business.Playlist = items;
            DisplayData();
        }

        private void DisplayData() {
            // Display custom column header when custom rating is selected.
            bool HasCustom = !string.IsNullOrEmpty(Settings.CustomColumn); // && business.Playlist.Where(v => v.Custom != null).Any());
            CustomColumn.Header = HasCustom ? business.GetRatingInitials(Settings.CustomColumn) : "";
            CustomColumn.Width = HasCustom ? 35 : 0;

            // Auto-adjust title column width.
            double ColWidth = ListGridView.Columns.Where(c => c != TitleColumn).Sum(c => c.ActualWidth);
            // The last part is to adjust so that it fits with either 100% or 150% zoom
            TitleColumn.Width = VideosView.ActualWidth - ColWidth - 28;
            // TitleColumn.Width = (VideosView.ActualWidth / Business.Settings.Zoom) - ColWidth - 28 * Math.Pow(Business.Settings.Zoom, 1.5);

            int CurrentSelection = VideosView.SelectedIndex;
            // Sort after loading for Status column.
            List<VideoListItem> SortList = business.Playlist;
            if (lastHeaderClicked == StatusColumn) {
                if (lastDirection == ListSortDirection.Ascending)
                    SortList = SortList.OrderBy(v => v.StatusText).ToList();
                else
                    SortList = SortList.OrderByDescending(v => v.StatusText).ToList();
            }
            VideosView.ItemsSource = SortList;

            // Maintain selection.
            if (!AllowMultiSelect) {
                if (CurrentSelection < 0)
                    VideosView.SelectedIndex = 0;
                else if (CurrentSelection < VideosView.Items.Count)
                    VideosView.SelectedIndex = CurrentSelection;
                else
                    VideosView.SelectedIndex = VideosView.Items.Count - 1;
            }
        }


        private async void VideosView_ColumnHeaderClick(object sender, RoutedEventArgs e) {
            GridViewColumnHeader headerClicked =
                  e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null && headerClicked.Role != GridViewColumnHeaderRole.Padding && !string.IsNullOrEmpty((string)headerClicked.Column.Header)) {
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

        private void VideosView_ItemDoubleClick(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (ItemDoubleClick != null)
                    ItemDoubleClick(this, new EventArgs());
        }

        public void EditSelection() {
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

        public async void EditForm_Closed(Media result) {
            if (result != null) {
                VideoListItem OldItem = (VideoListItem)VideosView.SelectedItem;
                business.RefreshPlaylist(result, OldItem != null ? OldItem.FileName : null);
                await LoadDataAsync();
            }
        }

        private void VideosView_ItemRightButtonUp(object sender, MouseButtonEventArgs e) {
            if (VideosView.SelectedItem != null && PopupEnabled) {
                FrameworkElement element = (FrameworkElement)e.OriginalSource;
                ListViewItem Item = (ListViewItem)VideosView.ItemContainerGenerator.ContainerFromItem(element.DataContext);
                if (Item != null)
                    ShowEditFormPopup((VideoListItem)Item.DataContext, sender as UIElement);
            }
        }

        private void VideosView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (AllowMultiSelect) {
                foreach (VideoListItem item in e.RemovedItems) {
                    selectionList.Remove(item);
                }

                foreach (VideoListItem item in e.AddedItems) {
                    selectionList.Add(item);
                }
            }

            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgs());
        }

        private void SearchTextControl_PreviewKeyDown(object sender, KeyEventArgs e) {
            ListBox Obj = IsDetailView ? VideosView : CategoriesList;
            if (e.Key == Key.Down && Obj.SelectedIndex < Obj.Items.Count)
                Obj.SelectedIndex += 1;
            if (e.Key == Key.Up && Obj.SelectedIndex > 0)
                Obj.SelectedIndex -= 1;
            Obj.ScrollIntoView(Obj.SelectedItem);
        }

        /// <summary>
        /// Show details when pressing Enter, close when pressing Escape.
        /// </summary>
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && !IsDetailView) {
                e.Handled = true;
                await ShowDetailsAsync();
            }
            if (e.Key == Key.Escape && IsDetailView && !IsLoading) {
                e.Handled = true;
                await CloseDetailsAsync();
            }
        }

        public int PlayListCount {
            get {
                if (business.Playlist != null)
                    return business.Playlist.Count();
                else
                    return 0;
            }
        }

        private async void CategoriesList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            await ShowDetailsAsync();
        }

        /// <summary>
        /// Shows the details view.
        /// </summary>
        public async Task ShowDetailsAsync() {
            if (CategoriesList.SelectedIndex < 0)
                return;

            SearchCategoryItem Item = CategoriesList.SelectedItem as SearchCategoryItem;
            if (Item.FilterType == SearchFilterEnum.None)
                return;

            if (Item.FilterValue == null && (Item.FilterType == SearchFilterEnum.Artist || Item.FilterType == SearchFilterEnum.Category || Item.FilterType == SearchFilterEnum.Element)) {
                // Change category group type.
                SearchGroupType = Item.FilterType;
                await LoadCategoriesAsync();
            } else {
                // Open details view.
                IsDetailView = true;
                Settings.IsInDatabase = (Item.FilterType != SearchFilterEnum.Files);

                bool HasArtistColumn = (Item.FilterType != SearchFilterEnum.Artist || Item.FilterValue != "") && Item.FilterType != SearchFilterEnum.Files;
                ArtistColumn.Width = HasArtistColumn ? 80 : 0;
                Settings.FilterType = Item.FilterType;
                Settings.FilterValue = Item.FilterValue;
                Settings.DisplayCustomRating = (Item.FilterType == SearchFilterEnum.Element) ? Item.FilterValue : null;

                if (VideosView.HasItems) {
                    VideosView.SelectedIndex = 0;
                    VideosView.ScrollIntoView(VideosView.Items[0]);
                }
                await LoadDataAsync();
                StoryboardOpenDetail.Seek(this, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                StoryboardOpenDetail.Begin();
                ShowCategoryLabel();
            }
        }

        private void ShowCategoryLabel() {
            // Get selected item rectangle.
            ListBoxItem SelectedItem = CategoriesList.ItemContainerGenerator.ContainerFromIndex(CategoriesList.SelectedIndex) as ListBoxItem;
            if (SelectedItem == null || !IsItemVisible(SelectedItem, CategoriesList)) {
                int VisibleItemIndex = (int)CategoriesList.FindVisualChildren<VirtualizingStackPanel>().FirstOrDefault().VerticalOffset;
                SelectedItem = CategoriesList.ItemContainerGenerator.ContainerFromIndex(VisibleItemIndex) as ListBoxItem;
            }

            Point transform = SelectedItem.TransformToVisual(GridCategories).Transform(new Point());
            System.Windows.Rect SelectedItemBounds = VisualTreeHelper.GetDescendantBounds(SelectedItem);
            SelectedItemBounds.Offset(transform.X, transform.Y);

            // Prepare box for display.
            SelectedCategoryLabel.Content = ((SearchCategoryItem)CategoriesList.SelectedItem).Text;
            SelectedCategoryLabel.Margin = new Thickness(SelectedItemBounds.Left, SelectedItemBounds.Top, 0, 0);
            SelectedCategoryLabel.Height = SelectedItemBounds.Bottom - SelectedItemBounds.Top;
            SelectedCategoryLabel.Width = SelectedItemBounds.Right - SelectedItemBounds.Left;
        }

        private bool IsItemVisible(FrameworkElement element, FrameworkElement container) {
            if (!element.IsVisible)
                return false;

            System.Windows.Rect bounds =
                element.TransformToAncestor(container).TransformBounds(new System.Windows.Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new System.Windows.Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        /// <summary>
        /// Closes the details view.
        /// </summary>
        public async Task CloseDetailsAsync() {
            Settings.Search = "";
            await LoadCategoriesAsync();
            IsDetailView = false;

            StoryboardCloseDetail.Seek(this, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            StoryboardCloseDetail.Begin();
            CategoriesList.Focus();

            SelectedCategoryLabel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Close details view when clicking on grayed list.
        /// </summary>
        private async void CategoriesList_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (IsDetailView && !IsLoading) {
                e.Handled = true;
                await CloseDetailsAsync();
            }
        }

        private void Storyboard_Completed(object sender, EventArgs e) {
            FocusControl?.Focus();
        }
    }
}
