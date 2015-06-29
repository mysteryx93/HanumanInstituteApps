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
using System.Windows.Shapes;
using Business;
using DataAccess;
using System.ComponentModel;
using Microsoft.Win32;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class ImportWindow : Window {
        public static void Instance() {
            OpenFileDialog OpenDlg = new OpenFileDialog();
            OpenDlg.DefaultExt = ".xml";
            OpenDlg.Filter = "Xml Files|*.xml";

            if (OpenDlg.ShowDialog() == true) {
                ImportWindow NewForm = new ImportWindow();

                try {
                    NewForm.MediaList.DisplayData(NewForm.business.ImportPreview(OpenDlg.FileName));
                } catch {
                    MessageBox.Show("Cannot load file.", "Result");
                    return;
                }

                SessionCore.Instance.Windows.Show(NewForm);
            }
        }

        private WindowHelper helper;
        SyncBusiness business = new SyncBusiness();

        public ImportWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            MediaList.EditSelection();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e) {
            MediaList.VideosView.SelectAll();
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e) {
            MediaList.VideosView.SelectedItem = null;
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e) {
            List<Guid> Selection = SelectedItems.Select(s => s.MediaId.Value).ToList();
            if (Selection.Count > 0) {
                ImportButton.IsEnabled = false;
                Progress<int> progress = new Progress<int>();
                progress.ProgressChanged += (psender, pe) => {
                    ProgressText.Text = string.Format("{0}/{1}", pe, Selection.Count);
                };
                await business.ImportToDatabase(Selection, progress);

                Task AutoBindTask = EditPlaylistBusiness.AutoBindFilesAsync();
                MessageBox.Show("Import completed", "Result");
                this.Close();
                await AutoBindTask;
            }
        }

        private List<VideoListItem> SelectedItems {
            get {
                return ((System.Collections.IList)MediaList.VideosView.SelectedItems).Cast<VideoListItem>().ToList();
            }
        }
    }
}
