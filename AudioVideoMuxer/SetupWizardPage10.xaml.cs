using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Business;

namespace AudioVideoMuxer {
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SetupWizardPage10 : Page, IWizardPage {
        public SetupWizardPage10() {
            InitializeComponent();
        }

        public MainWindow Owner { get; set; }
        ObservableCollection<FileItem> files = new ObservableCollection<FileItem>();
        ObservableCollection<FfmpegStream> fileStreams = new ObservableCollection<FfmpegStream>();

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            FilesList.DataContext = files;
            StreamsList.DataContext = fileStreams;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            string Result = FileFolderDialog.ShowFileDialog("", "");
            if (!string.IsNullOrEmpty(Result) && !files.Any(f => f.Path == Result)) {
                string FileName = Path.GetFileName(Result);

                List<FfmpegStream> FileInfo = FfmpegBusiness.GetStreamList(Result);
                foreach (FfmpegStream item in FileInfo) {
                    fileStreams.Add(item);
                }

                //MediaInfoReader InfoReader = new MediaInfoReader();
                //InfoReader.LoadInfo(Result);

                //string StreamFormat = InfoReader.GetVideoFormat(0);
                //int Index = 0;
                //while (!string.IsNullOrEmpty(StreamFormat)) {
                //    HasStream = true;
                //    fileStreams.Add(new FfmpegStream(Result, FileName, FfmpegStreamType.Video, Index, StreamFormat));
                //    Index++;
                //    StreamFormat = InfoReader.GetVideoFormat(Index);
                //}

                //StreamFormat = InfoReader.GetAudioFormat(0);
                //Index = 0;
                //while (!string.IsNullOrEmpty(StreamFormat)) {
                //    HasStream = true;
                //    fileStreams.Add(new FfmpegStream(Result, FileName, FfmpegStreamType.Audio, Index, StreamFormat));
                //    Index++;
                //    StreamFormat = InfoReader.GetAudioFormat(Index);
                //}

                if (FileInfo.Count > 0)
                    files.Add(new FileItem(Result, FileName));
                else
                    MessageBox.Show("No video or audio stream found in file.", "Validation");
            }
        }

        public bool Validate() {
            Owner.Business.Clear();
            foreach (FfmpegStream item in fileStreams.Where(s => s.IsChecked)) {
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
