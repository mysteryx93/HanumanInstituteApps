using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.WpfCommon;

namespace HanumanInstitute.MediaMuxer {
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SetupWizardPage20 : Page, IWizardPage {
        public SetupWizardPage20() {
            InitializeComponent();
        }

        public MainWindow Owner { get; set; }
        ObservableCollection<FileItem> files = new ObservableCollection<FileItem>();
        ObservableCollection<UIFileStream> sourceStreams = new ObservableCollection<UIFileStream>();

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            FilesList.DataContext = files;
            SourceStreamsList.DataContext = sourceStreams;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            string[] ResultList = FileFolderDialog.ShowFileDialogMultiple("", "");
            foreach (string Result in ResultList) {
                if (!string.IsNullOrEmpty(Result) && !files.Any(f => f.Path == Result)) {
                    string FileName = Path.GetFileName(Result);

                    List<FFmpegStreamInfo> FileInfo = MediaInfo.GetFileInfo(Result, new ProcessStartOptions(FFmpegDisplayMode.None)).FileStreams;
                    if (files.Count == 0) {
                        // Store streams of first file.
                        foreach (FFmpegStreamInfo item in FileInfo) {
                            sourceStreams.Add(new UIFileStream(item, Result));
                        }
                    } else {
                        // Make sure streams match the first file.
                        bool Mismatch = false;
                        if (FileInfo.Count != sourceStreams.Count)
                            Mismatch = true;
                        else {
                            for (int i = 0; i < FileInfo.Count; i++) {
                                if (FileInfo[i].StreamType != sourceStreams[i].Type || FileInfo[i].Format != sourceStreams[i].Format) {
                                    Mismatch = true;
                                    MessageBox.Show("Streams don't match the first media file.", "Validation");
                                    break;
                                }
                            }
                        }
                        if (Mismatch)
                            break;
                    }

                    if (FileInfo.Count > 0)
                        files.Add(new FileItem(Result, FileName));
                    else
                        MessageBox.Show("No video or audio stream found in file.", "Validation");
                }
            }
        }

        public bool Validate() {
            Owner.Business.Clear();
            foreach (FileItem item in files) {
                Owner.Business.Files.Add(item);
            }
            if (Owner.Business.Files.Count() > 1)
                return true;
            else {
                MessageBox.Show("You must select at least two files.", "Validation");
                return false;
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e) {
            int Sel = FilesList.SelectedIndex;
            if (Sel > 0) {
                FileItem Selection = files[Sel];
                files.RemoveAt(Sel);
                files.Insert(Sel - 1, Selection);
                FilesList.SelectedIndex = Sel - 1;
            }
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e) {
            int Sel = FilesList.SelectedIndex;
            if (Sel < files.Count() - 1) {
                FileItem Selection = files[Sel];
                files.RemoveAt(Sel);
                files.Insert(Sel + 1, Selection);
                FilesList.SelectedIndex = Sel + 1;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            int Sel = FilesList.SelectedIndex;
            if (Sel > -1) {
                files.RemoveAt(Sel);
                if (files.Count == 0)
                    sourceStreams.Clear();
                else if (Sel < files.Count)
                    FilesList.SelectedIndex = Sel;
                else
                    FilesList.SelectedIndex = files.Count - 1;
            }
        }
    }
}
