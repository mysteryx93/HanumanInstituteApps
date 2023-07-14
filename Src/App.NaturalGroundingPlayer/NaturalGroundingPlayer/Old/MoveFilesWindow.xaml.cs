using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Business;
using DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MoveFilesWindow.xaml
    /// </summary>
    public partial class MoveFilesWindow : Window {
        public static void Instance(Action callback) {
            MoveFilesWindow NewForm = new MoveFilesWindow();
            NewForm.callback = callback;
            SessionCore.Instance.Windows.Show(NewForm);
        }

        private List<MoveFileItem> selectionList = new List<MoveFileItem>();
        private MoveFilesBusiness business = new MoveFilesBusiness();
        private DispatcherTimer timerChangeFilters;
        protected Action callback;
        private IMediaPlayerBusiness player = SessionCore.Instance.GetNewPlayer();
        private WindowHelper helper;

        public MoveFilesWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            timerChangeFilters = new DispatcherTimer();
            timerChangeFilters.Interval = TimeSpan.FromSeconds(1);
            timerChangeFilters.Tick += timerChangeFilters_Tick;
            timerChangeFilters.Start();

            business.LoadList();
            DisplayData();

            player.SetPath();
            player.AllowClose = true;
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e) {
            timerChangeFilters.Stop();
            timerChangeFilters.Start();
        }

        private void timerChangeFilters_Tick(object sender, EventArgs e) {
            timerChangeFilters.Stop();
            DisplayData();
        }

        public void DisplayData() {
            FilesView.ItemsSource = business.FilterList(SearchText.Text);
        }

        private void FilesView_ItemDoubleClick(object sender, MouseButtonEventArgs e) {
            if (FilesView.SelectedItem != null) {
                MoveFileItem Item = (MoveFileItem)FilesView.SelectedItem;
                EditVideoWindow.Instance(Item.VideoId, null, delegate(Media result) {
                    if (result != null) {
                        business.Load(result.MediaId);
                        DisplayData();
                    }
                });
            }
        }

        private void FilesView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (MoveFileItem item in e.RemovedItems) {
                selectionList.Remove(item);
            }

            foreach (MoveFileItem item in e.AddedItems) {
                selectionList.Add(item);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (callback != null)
                callback();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e) {
            FilesView.SelectAll();
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e) {
            FilesView.SelectedItem = null;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MoveFilesButton_Click(object sender, RoutedEventArgs e) {
            business.MoveFiles(selectionList);
            business.LoadList();
            DisplayData();
        }
    }
}
