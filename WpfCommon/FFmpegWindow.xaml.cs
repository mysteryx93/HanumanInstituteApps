using System;
using System.Windows;
using System.Windows.Media;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.WpfCommon {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegWindow : Window {
        public static void Instance(FFmpegProcess host) {
            FFmpegWindow F = new FFmpegWindow();
            F.host = host;
            F.Show();
        }

        private FFmpegProcess host;

        public FFmpegWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            SetPageTitle(null);
            OutputTextBox.AppendText(host.CommandWithArgs + Environment.NewLine + Environment.NewLine);

            host.DataReceived += FFmpeg_DataReceived;
            host.InfoUpdated += FFmpeg_InfoUpdated;
            host.ProgressUpdated += FFmpeg_ProgressUpdated;
            host.Completed += FFmpeg_Completed;
        }

        private void SetPageTitle(string status) {
            string PageTitle = string.IsNullOrEmpty(host.Options.DisplayTitle) ? "FFmpeg Work in Progress" : host.Options.DisplayTitle;
            if (!string.IsNullOrEmpty(status))
                PageTitle = string.Format("{0} ({1})", PageTitle, status);
            this.Title = PageTitle;
        }

        private void FFmpeg_InfoUpdated(object sender, EventArgs e) {
            Dispatcher.Invoke(() => WorkProgressBar.Maximum = host.FrameCount);
        }

        private void FFmpeg_ProgressUpdated(object sender, FFmpeg.ProgressUpdatedEventArgs e) {
            Dispatcher.Invoke(() => {
                WorkProgressBar.Value = e.Progress.Frame;
                PercentText.Text = (WorkProgressBar.Value / WorkProgressBar.Maximum).ToString("p1");
                SetPageTitle(PercentText.Text);
                FpsText.Text = e.Progress.Fps.ToString();

                if (e.Progress.EstimatedTimeLeft != TimeSpan.Zero)
                    TimeLeftText.Text = e.Progress.EstimatedTimeLeft.ToString(e.Progress.EstimatedTimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss");
            });
        }

        private void FFmpeg_DataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
            Dispatcher.Invoke(() => {
                OutputTextBox.AppendText(e.Data + Environment.NewLine);
                OutputTextBox.ScrollToEnd();
            });
        }

        private void FFmpeg_Completed(object sender, FFmpeg.CompletedEventArgs e) {
            Dispatcher.Invoke(() => {
                CompletedText.Text = e.Status.ToString();
                SetPageTitle(CompletedText.Text);
                CompletedText.Foreground = new SolidColorBrush((e.Status == FFmpeg.CompletionStatus.Success) ? Color.FromRgb(0x07, 0xC9, 0x07) : Colors.Red);
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            host.Cancel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            host.Cancel();
        }
    }
}
