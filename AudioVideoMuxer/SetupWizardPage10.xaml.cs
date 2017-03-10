using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.WpfCommon;

namespace EmergenceGuardian.AudioVideoMuxer {
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SetupWizardPage10 : Page, IWizardPage {
        public SetupWizardPage10() {
            InitializeComponent();
        }

        public MainWindow Owner { get; set; }
        ObservableCollection<FileItem> files = new ObservableCollection<FileItem>();
        ObservableCollection<UIFileStream> fileStreams = new ObservableCollection<UIFileStream>();

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            FilesList.DataContext = files;
            StreamsList.DataContext = fileStreams;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            string Result = FileFolderDialog.ShowFileDialog("", "");
            if (!string.IsNullOrEmpty(Result) && !files.Any(f => f.Path == Result)) {
                string FileName = Path.GetFileName(Result);

                List<FFmpegStreamInfo> FileInfo = MediaMuxer.GetFileInfo(Result, new ProcessStartOptions(FFmpegDisplayMode.None)).FileStreams;
                if (FileInfo != null && FileInfo.Count > 0) {
                    files.Add(new FileItem(Result, FileName));
                    foreach (FFmpegStreamInfo item in FileInfo) {
                        fileStreams.Add(new UIFileStream(item, Result));
                    }
                } else
                    MessageBox.Show("No video or audio stream found in file.", "Validation");
            }
        }

        public bool Validate() {
            Owner.Business.Clear();
            foreach (UIFileStream item in fileStreams.Where(s => s.IsChecked)) {
                Owner.Business.FileStreams.Add(item);
            }
            if (Owner.Business.FileStreams.Count() > 0)
                return true;
            else {
                MessageBox.Show("No stream selected.", "Validation");
                return false;
            }            
        }
    }
}
