using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NaturalGroundingPlayer;
using Business;
using DataAccess;

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
            MainWindow ActiveWindow = SessionCore.Instance.Windows.Current as MainWindow;
            MediaEncoderBusiness business = ActiveWindow.business;
            try {
                OkButton.IsEnabled = false;
                if (ResumeOption.IsChecked == true) {
                    business.AddJobToQueue(jobInfo.Settings);
                } else if (RestartOption.IsChecked == true) {
                    AvisynthTools.EditStartPosition(jobInfo.Settings.ScriptFile, 0, 0);
                    PathManager.DeleteOutputFiles(jobInfo.Settings.JobIndex);
                    business.AddJobToQueue(jobInfo.Settings);
                } else if (DeleteOption.IsChecked == true) {
                    PathManager.DeleteJobFiles(jobInfo.Settings.JobIndex);
                } else if (ChangeOption.IsChecked == true) {
                    ActiveWindow.HidePreview();
                    await business.MovePreviewFilesAsync(jobInfo.Settings);
                    ActiveWindow.SetEncodeSettings(jobInfo.Settings);
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
