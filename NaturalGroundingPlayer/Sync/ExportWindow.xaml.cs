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
using EmergenceGuardian.NaturalGroundingPlayer.Business;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;
using System.ComponentModel;
using Microsoft.Win32;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class ExportWindow : Window {
        public static void Instance() {
            ExportWindow NewForm = new ExportWindow();
            SessionCore.Instance.Windows.Show(NewForm);
        }

        WindowHelper helper;

        public ExportWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            MediaList.Settings.SetCondition(FieldConditionEnum.IsPersonal, true);
            await Task.Delay(1);
            await MediaList.LoadDataAsync();
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

        private void ExportButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog SaveDlg = new SaveFileDialog();
            SaveDlg.OverwritePrompt = true;
            SaveDlg.DefaultExt = ".xml";
            SaveDlg.Filter = "Xml Files|*.xml";

            if (SaveDlg.ShowDialog() == true) {
                SyncBusiness Export = new SyncBusiness();
                Export.ExportToFile(SelectedItems.Select(v => v.MediaId.Value).ToList(), SaveDlg.FileName);
                MessageBox.Show("Export completed", "Result");
                this.Close();
            }
        }

        private List<VideoListItem> SelectedItems {
            get {
                return ((System.Collections.IList)MediaList.VideosView.SelectedItems).Cast<VideoListItem>().ToList();
            }
        }
    }
}
