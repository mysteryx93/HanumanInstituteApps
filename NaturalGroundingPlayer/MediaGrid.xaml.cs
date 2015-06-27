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

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MediaGrid.xaml
    /// </summary>
    public partial class MediaGrid : UserControl {
        private GridViewColumn lastHeaderClicked;
        private ListSortDirection lastDirection = ListSortDirection.Ascending;
        private List<VideoListItem> selectionList = new List<VideoListItem>();
        public EditPlaylistBusiness business = new EditPlaylistBusiness();
        public IMediaPlayerBusiness player;
        public SearchSettings Settings { get; set; }
        public bool IsolatedPlayer { get; set; }
        public bool PopupEnabled { get; set; }
        public bool IsPreferenceVisible { get; set; }
        public bool IsIntensityVisible { get; set; }
        public bool IsCustomVisible { get; set; }
        public bool IsStatusVisible { get; set; }
        public bool AllowMultiSelect { get; set; }
        public bool IsLoading { get; private set; }
        public event EventHandler DataLoaded;
        public event EventHandler ItemDoubleClick;
        public event EventHandler SelectionChanged;

        public MediaGrid() {
            InitializeComponent();
            Settings = new SearchSettings();
            PopupEnabled = true;
            IsPreferenceVisible = true;
            IsIntensityVisible = true;
            IsCustomVisible = true;
            IsStatusVisible = false;
            AllowMultiSelect = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
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

            // Grid column width does not include zoom.
            double ColWidth = ListGridView.Columns.Where(c => c != TitleColumn).Sum(c => c.ActualWidth);
            TitleColumn.Width = (this.ActualWidth / Business.Settings.Zoom) - ColWidth - 28 * Business.Settings.Zoom;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            lastHeaderClicked = ((GridView)VideosView.View).Columns[0];
            Window.GetWindow(this).Closed += MediaGrid_Closed;

            if (IsolatedPlayer) {
                player = SessionCore.Instance.GetNewPlayer();
                player.SetPath();
                player.AllowClose = true;
            }

            // Bind PreviewKeyDown to every TextBox on the page.
            foreach (TextBox item in Window.GetWindow(this).FindVisualChildren<TextBox>()) {
                item.PreviewKeyDown += SearchTextControl_PreviewKeyDown;
            }
        }

        void MediaGrid_Closed(object sender, EventArgs e) {
            if (player != null)
                player.Close();
        }

        public VideoListItem SelectedItem {
            get {
                return (VideoListItem)VideosView.SelectedItem;
            }
        }

        public async Task LoadDataAsync() {
            IsLoading = true;
            if (lastHeaderClicked.DisplayMemberBinding != null)
                Settings.OrderBy = ((Binding)lastHeaderClicked.DisplayMemberBinding).Path.Path;
            else
                Settings.OrderBy = lastHeaderClicked.HeaderStringFormat;
            Settings.OrderByDirection = lastDirection;
            await business.LoadPlaylistAsync(Settings, true);
            DisplayData();
            IsLoading = false;
            if (DataLoaded != null)
                DataLoaded(this, null);
        }

        public void Clear() {
            VideosView.ItemsSource = null;
        }

        private void DisplayData() {
            int CurrentSelection = VideosView.SelectedIndex;
            VideosView.ItemsSource = business.Playlist;

            // Display custom column header when custom rating is selected.
            if (string.IsNullOrEmpty(Settings.RatingCategory) || business.Playlist.Where(v => v.Custom != null).Any() == false)
                CustomColumn.Header = "";
            else
                CustomColumn.Header = business.GetRatingInitials(Settings.RatingCategory);

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

            if (headerClicked != null && headerClicked.Role != GridViewColumnHeaderRole.Padding && !string.IsNullOrEmpty((string)headerClicked.Column.Header) && (string)headerClicked.Column.Header != "Status") {
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
            EditVideoWindow.Instance(item.MediaId, item.FileName, player, EditForm_Closed);
        }

        private void ShowEditFormPopup(VideoListItem item, UIElement target) {
            EditVideoWindow.InstancePopup(target, PlacementMode.Mouse, item.MediaId, item.FileName, player, EditForm_Closed);
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
            if (e.Key == Key.Down && VideosView.SelectedIndex < VideosView.Items.Count)
                VideosView.SelectedIndex += 1;
            if (e.Key == Key.Up && VideosView.SelectedIndex > 0)
                VideosView.SelectedIndex -= 1;
            VideosView.ScrollIntoView(VideosView.SelectedItem);
        }

        public int PlayListCount {
            get {
                if (business.Playlist != null)
                    return business.Playlist.Count();
                else
                    return 0;
            }
        }
    }
}
