using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Business;
using System.Diagnostics;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MediaEncoderDeshakerPassWindow.xaml
    /// </summary>
    public partial class MediaEncoderDeshakerPassWindow : Window {
        public static void Instance(MediaEncoderBusiness business, MediaEncoderSettings settings, Process process) {
            MediaEncoderDeshakerPassWindow NewForm = new MediaEncoderDeshakerPassWindow();
            NewForm.business = business;
            NewForm.settings = settings;
            NewForm.process = process;
            SessionCore.Instance.Windows.ShowDialog(NewForm);
        }

        private WindowHelper helper;
        private MediaEncoderBusiness business;
        private MediaEncoderSettings settings;
        private Process process;
        private DispatcherTimer progressTimer = new DispatcherTimer();
        private FfmpegBusiness.ClipInfo clipInfo;

        public MediaEncoderDeshakerPassWindow() {
            InitializeComponent();
            helper = new NaturalGroundingPlayer.WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            progressTimer.Tick += ProgressTimer_Tick;
            progressTimer.Interval = new TimeSpan(0, 0, 1);
            progressTimer.Start();

            clipInfo = await Task.Run(() => FfmpegBusiness.GetClipInfo(settings, settings.DeshakerScript, true));
            ScanProgress.Maximum = clipInfo.FrameCount;
        }

        /// <summary>
        /// Track progress by looking at the size of the Deshaker Log. 94 bytes per line.
        /// </summary>
        private void ProgressTimer_Tick(object sender, EventArgs e) {
            if (process.HasExited)
                Close();
            else if (clipInfo != null && File.Exists(settings.DeshakerLog)) {
                long Length = new FileInfo(settings.DeshakerLog).Length;
                long Progress = Length / 94;
                StatusText.Text = string.Format("{0} / {1}", Progress.ToString(), clipInfo.FrameCount);
                ScanProgress.Value = Progress;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!process.HasExited)
                process.Kill();
        }
    }
}
