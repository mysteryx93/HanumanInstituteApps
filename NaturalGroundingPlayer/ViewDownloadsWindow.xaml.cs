using System;
using System.Windows;
using System.Windows.Input;
using Business;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for ViewDownloadsWindow.xaml
    /// </summary>
    public partial class ViewDownloadsWindow : Window {
        private WindowHelper helper;

        public ViewDownloadsWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
            SessionCore.Instance.Business.DownloadManager.DownloadAdded += DownloadManager_DownloadAdded;
        }

        /// <summary>
        /// Ensure this window is visible when a new download is added.
        /// </summary>
        private void DownloadManager_DownloadAdded(object sender, EventArgs e) {
            if (this.Visibility != System.Windows.Visibility.Visible)
                this.ShowAndPosition(ToolboxPosition.Right);
        }

        public void ShowAndPosition(ToolboxPosition position) {
            SessionCore.Instance.Windows.ShowToolbox(this, position);
        }

        private bool IsPlayerMode {
            get {
                Type CurrentWindow = SessionCore.Instance.Windows.Current.GetType();
                return CurrentWindow == typeof(MainWindow) || CurrentWindow == typeof(ManualPlayerWindow);
            }
        }

        private async void DownloadsView_ItemDoubleClick(object sender, MouseButtonEventArgs e) {
            DownloadItem Item = DownloadsView.SelectedItem as DownloadItem;
            if (Item != null && IsPlayerMode) {
                if (Item.IsCompleted) {
                    await SessionCore.Instance.Business.SetNextVideoFileAsync(PlayerMode.SpecialRequest, Item.Request.FileName);
                    await SessionCore.Instance.Business.SkipVideoAsync();
                } else {
                    menuEdit_Click(null, null);
                }
            }
        }

        private void DownloadsView_ItemRightButtonUp(object sender, MouseButtonEventArgs e) {
            DownloadsView.ContextMenu = null;
            DownloadItem Item = DownloadsView.SelectedItem as DownloadItem;
            if (Item != null && IsPlayerMode) {
                string MenuKey = null;
                if (Item.IsCompleted || Item.IsCanceled)
                    MenuKey = "ContextMenuEdit";
                else if (!Item.IsCanceled)
                    MenuKey = "ContextMenuCancel";
                if (MenuKey != null)
                    ShowContextMenu((ContextMenu)FindResource(MenuKey), (UIElement)sender);
            }
        }

        private void ShowContextMenu(ContextMenu menu, UIElement target) {
            DownloadsView.ContextMenu = menu;
            menu.Placement = PlacementMode.Mouse;
            menu.PlacementTarget = target;
            menu.IsOpen = true;
        }

        private void menuCancel_Click(object sender, RoutedEventArgs e) {
            DownloadItem Item = DownloadsView.SelectedItem as DownloadItem;
            if (Item != null && !Item.IsCanceled && !Item.IsCompleted)
                Item.Status = DownloadStatus.Canceled;
        }

        private void menuEdit_Click(object sender, RoutedEventArgs e) {
            DownloadItem Item = DownloadsView.SelectedItem as DownloadItem;
            if (Item != null)
                EditVideoWindow.Instance(Item.Request.MediaId, null, null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }
    }
}
