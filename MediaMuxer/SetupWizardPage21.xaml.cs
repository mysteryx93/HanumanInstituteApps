using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HanumanInstitute.WpfCommon;

namespace HanumanInstitute.MediaMuxer {
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SetupWizardPage21 : Page, IWizardPage {
        public SetupWizardPage21() {
            InitializeComponent();
        }

        public MainWindow Owner { get; set; }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            Owner.IsLastPage = true;
            SelectedStreams.DataContext = Owner.Business.Files;
        }

        public bool Validate() {
            bool Success = false;
            if (OutputFileTextBox.Text.Trim().Length > 0) { 
                try {
                    File.Create(OutputFileTextBox.Text).Close();
                    File.Delete(OutputFileTextBox.Text);
                    Owner.Business.OutputFile = OutputFileTextBox.Text;
                    Success = true;
                } catch { }
            }

            if (!Success)
                MessageBox.Show("Please enter a valid output file name.", "Validation");
            return Success;
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e) {
            string Filter = OutputContainerTextBox.Text.Trim().Length > 0 ? string.Format("{0}|*.{0}", OutputContainerTextBox.Text.Trim()) : "";
            string Result = FileFolderDialog.ShowSaveFileDialog("", Filter);
            if (Result != null)
                OutputFileTextBox.Text = Result;
        }
    }
}
