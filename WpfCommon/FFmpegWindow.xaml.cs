using System;
using System.Windows;
using System.Windows.Media;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.WpfCommon {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegWindow : Window, IUserInterface {
        public static FFmpegWindow Instance(Window parent, string title, bool autoClose) {
            FFmpegWindow F = new FFmpegWindow();
            F.Owner = parent;
            F.title = title;
            F.autoClose = autoClose;
            F.Show();
            return F;
        }

        private FFmpegProcess host;
        private FFmpegProcess task;
        private bool autoClose;
        private string title { get; set; }
        private TimeLeftCalculator timeCalc;

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
                    host.InfoUpdated += FFmpeg_InfoUpdated;
                    host.StatusUpdated += FFmpeg_ProgressUpdated;
                    host.Completed += FFmpeg_Completed;
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
                            if (autoClose)
                                this.Close();
                        });
                    };
                }
            });
        }

        private void SetPageTitle(string status) {
            this.Title = string.IsNullOrEmpty(status) ? title : string.Format("{0} ({1})", title, status);
        }

        private void FFmpeg_InfoUpdated(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                WorkProgressBar.Maximum = host.FrameCount + host.Options.ResumePos;
                timeCalc = new TimeLeftCalculator(host.FrameCount + host.Options.ResumePos);
            });
        }

        private bool EstimatedTimeLeftToggle = false;
        private void FFmpeg_ProgressUpdated(object sender, FFmpeg.StatusUpdatedEventArgs e) {
            Dispatcher.Invoke(() => {
                WorkProgressBar.Value = e.Status.Frame + host.Options.ResumePos;
                PercentText.Text = (WorkProgressBar.Value / WorkProgressBar.Maximum).ToString("p1");
                SetPageTitle(PercentText.Text);
                FpsText.Text = e.Status.Fps.ToString();

                // Time left will be updated only 1 out of 2 to prevent changing too quick.
                EstimatedTimeLeftToggle = !EstimatedTimeLeftToggle;
                if (EstimatedTimeLeftToggle) {
                    TimeSpan TimeLeft = timeCalc?.Calculate(e.Status.Frame + host.Options.ResumePos) ?? TimeSpan.Zero;
                    if (TimeLeft > TimeSpan.Zero)
                        TimeLeftText.Text = TimeLeft.ToString(TimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss");
                }
            });
        }

        private void FFmpeg_Completed(object sender, FFmpeg.CompletedEventArgs e) {
            Dispatcher.Invoke(() => {
                FFmpegProcess Proc = sender as FFmpegProcess;
                if (e.Status == CompletionStatus.Error && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2yuv.exe"))
                    FFmpegErrorWindow.Instance(Owner, Proc);
                if (autoClose)
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
    }
}
