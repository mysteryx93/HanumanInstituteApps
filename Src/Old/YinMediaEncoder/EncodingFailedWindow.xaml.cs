using System;
using System.Windows;
using EmergenceGuardian.MediaEncoder;

namespace YinMediaEncoder {
    /// <summary>
    /// Interaction logic for CompletedWindow.xaml
    /// </summary>
    public partial class EncodingFailedWindow : Window {
        public static void Instance(Window parent, EncodingCompletedEventArgs jobInfo) {
            EncodingFailedWindow NewForm = new EncodingFailedWindow();
            NewForm.Owner = parent;
            NewForm.jobInfo = jobInfo;
            NewForm.Show();
        }

        protected EncodingCompletedEventArgs jobInfo;
        private WindowHelper helper;

        public EncodingFailedWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DataContext = jobInfo;
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e) {
            MediaEncoderBusiness business = MainWindow.Instance.business;
            try {
                OkButton.IsEnabled = false;
                if (ResumeOption.IsChecked == true) {
                    business.AddJobToQueue(jobInfo.Settings);
                } else if (RestartOption.IsChecked == true) {
                    EncoderBusiness.EditStartPosition(jobInfo.Settings.ScriptFile, 0, 0);
                    PathManager.DeleteOutputFiles(jobInfo.Settings.JobIndex);
                    business.AddJobToQueue(jobInfo.Settings);
                } else if (DeleteOption.IsChecked == true) {
                    PathManager.DeleteJobFiles(jobInfo.Settings.JobIndex);
                } else if (ChangeOption.IsChecked == true) {
                    MainWindow.Instance.HidePreview();
                    await business.MovePreviewFilesAsync(jobInfo.Settings);
                    MainWindow.Instance.SetEncodeSettings(jobInfo.Settings);
                    PathManager.DeleteJobFiles(jobInfo.Settings.JobIndex);
                    jobInfo.Settings.JobIndex = -1;
                }
                this.Close();
                Owner?.Focus();
            }
            catch (Exception ex) {
                OkButton.IsEnabled = true;
                MessageBox.Show(this, ex.Message, "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Option_Click(object sender, RoutedEventArgs e) {
            OkButton.IsEnabled = true;
        }
    }
}
