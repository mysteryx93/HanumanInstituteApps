using System;
using System.Windows;
using System.Windows.Media;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.WpfCommon {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegWindow : Window, IUserInterface {
        public static FFmpegWindow Instance(Window parent, string title) {
            FFmpegWindow F = new FFmpegWindow();
            F.Owner = parent;
            F.title = title;
            F.Show();
            return F;
        }

        private FFmpegProcess host;
        private FFmpegProcess task;
        private string title { get; set; }

        public void Stop() {
            Dispatcher.Invoke(() => this.Close());
        }

        public FFmpegWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            SetPageTitle(null);
        }

        public void DisplayTask(FFmpegProcess taskArg) {
            Dispatcher.Invoke(() => {
                if (taskArg.Options.IsMainTask) {
                    host = taskArg;
                    host.DataReceived += FFmpeg_DataReceived;
                    host.InfoUpdated += FFmpeg_InfoUpdated;
                    host.ProgressUpdated += FFmpeg_ProgressUpdated;
                    host.Completed += FFmpeg_Completed;
                    OutputTextBox.AppendText(host.CommandWithArgs + Environment.NewLine + Environment.NewLine);
                    PercentText.Text = 0.ToString("p1");
                    SetPageTitle(PercentText.Text);
                } else {
                    task = taskArg;
                    TaskStatusText.Text = task.Options.Title;
                    task.Completed += (sender, e) => {
                        FFmpegProcess Proc = (FFmpegProcess)sender;
                        Dispatcher.Invoke(() => {
                            if (e.Status == CompletionStatus.Error && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2yuv.exe"))
                                FFmpegErrorWindow.Instance(Owner, Proc);
                            TaskStatusText.Text = "";
                            task = null;
                        });
                    };
                }
            });
        }

        private void SetPageTitle(string status) {
            this.Title = string.IsNullOrEmpty(status) ? title : string.Format("{0} ({1})", title, status);
        }

        private void FFmpeg_InfoUpdated(object sender, EventArgs e) {
            Dispatcher.Invoke(() => WorkProgressBar.Maximum = host.FrameCount + host.Options.ResumePos);
        }

        private bool EstimatedTimeLeftToggle = false;
        private void FFmpeg_ProgressUpdated(object sender, FFmpeg.ProgressUpdatedEventArgs e) {
            Dispatcher.Invoke(() => {
                WorkProgressBar.Value = e.Progress.Frame + host.Options.ResumePos;
                PercentText.Text = (WorkProgressBar.Value / WorkProgressBar.Maximum).ToString("p1");
                SetPageTitle(PercentText.Text);
                FpsText.Text = e.Progress.Fps.ToString();

                // Time left will be updated only 1 out of 2 to prevent changing too quick.
                EstimatedTimeLeftToggle = !EstimatedTimeLeftToggle;
                if (e.Progress.EstimatedTimeLeft != TimeSpan.Zero && EstimatedTimeLeftToggle)
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
                //CompletedText.Text = e.Status.ToString();
                //SetPageTitle(CompletedText.Text);
                //CompletedText.Foreground = new SolidColorBrush((e.Status == FFmpeg.CompletionStatus.Success) ? Color.FromRgb(0x07, 0xC9, 0x07) : Colors.Red);
                if (e.Status == CompletionStatus.Error)
                    FFmpegErrorWindow.Instance(Owner, (FFmpegProcess)sender);
                this.Close();
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            host?.Cancel();
            task?.Cancel();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            this.Height += 150;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e) {
            this.Height -= 150;
        }
    }
}
