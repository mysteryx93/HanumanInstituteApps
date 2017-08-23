using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading;
using EmergenceGuardian.FFmpeg;
using System.Threading.Tasks;

namespace EmergenceGuardian.WpfCommon {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegWindow : Window, IUserInterface {
        public static FFmpegWindow Instance(Window parent, string title, bool autoClose, FFmpegUserInterfaceManager manager) {
            FFmpegWindow F = new FFmpegWindow();
            F.Owner = parent;
            F.title = title;
            F.autoClose = autoClose;
            F.manager = manager;
            F.Show();
            return F;
        }

        private List<FFmpegProcess> hosts = new List<FFmpegProcess>();
        private FFmpegProcess task;
        private bool autoClose;
        private object jobId;
        private string title;
        private FFmpegUserInterfaceManager manager;
        private TimeLeftCalculator timeCalc = new TimeLeftCalculator(0, 30);
        private long FrameTotal;
        private long FrameTodo;
        private long FrameDone;
        private int ClosingState; // 0=Running, 1=Closing processes (window open), 2=Closing Window

        public void Stop() {
            Dispatcher.Invoke(() => {
                ClosingState = HasRunningHosts ? 0 : 2;
                this.Close();
            });
        }

        public FFmpegWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            SetPageTitle(null);
        }

        public void DisplayTask(FFmpegProcess taskArg) {
            if (manager.AppExited)
                return;
            Dispatcher.Invoke(() => {
                jobId = taskArg.Options.JobId;
                if (taskArg.Options.IsMainTask) {
                    hosts.Add(taskArg);
                    FrameTotal = taskArg.Options.TotalFrameCount > 0 ?
                        taskArg.Options.TotalFrameCount : hosts.Max(h => h.FrameCount + h.Options.ResumePos);
                    FrameTodo = hosts.Sum(h => h.FrameCount);
                    if (taskArg.Options.TotalFrameCount > 0)
                        FrameTodo += FrameTotal - hosts.Max(h => h.FrameCount + h.Options.ResumePos);
                    FrameDone = FrameTotal - FrameTodo;
                    WorkProgressBar.Maximum = FrameTodo > 0 ? FrameTotal : 1;
                    timeCalc.FrameCount = FrameTodo;

                    taskArg.InfoUpdated += FFmpeg_InfoUpdated;
                    taskArg.StatusUpdated += FFmpeg_ProgressUpdated;
                    taskArg.Completed += FFmpeg_Completed;
                    //if (taskArg.Options.ResumePos > 0)
                    //    PercentText.Text = (taskArg.Options.ResumePos / WorkProgressBar.Maximum).ToString("p1");
                    //else
                    if (PercentText.Text.Length == 0) {
                        PercentText.Text = 0.ToString("p1");
                        SetPageTitle(PercentText.Text);
                    }
                } else {
                    task = taskArg;
                    task.Completed += FFmpeg_Completed;
                    TaskStatusText.Text = task.Options.Title;
                }
            });
        }

        private void SetPageTitle(string status) {
            this.Title = string.IsNullOrEmpty(status) ? title : string.Format("{0} ({1})", title, status);
        }

        private void FFmpeg_InfoUpdated(object sender, EventArgs e) {
            if (manager.AppExited)
                return;
            Dispatcher.Invoke(() => {
                if (WorkProgressBar.Maximum == 1 && hosts.Count == 1) {
                    // If we're running FFMPEG directly without Avisynth, FrameCount isn't known ahead of time.
                    FrameTotal = hosts.Max(h => h.FrameCount + h.Options.ResumePos);
                    FrameTodo = hosts.Sum(h => h.FrameCount);
                    FrameDone = FrameTotal - FrameTodo;

                    WorkProgressBar.Maximum = FrameTotal;
                    timeCalc.FrameCount = FrameTotal;
                }
            });
        }

        private int EstimatedTimeLeftToggle = 0;
        private void FFmpeg_ProgressUpdated(object sender, FFmpeg.StatusUpdatedEventArgs e) {
            if (manager.AppExited)
                return;
            Dispatcher.Invoke(() => {
                FFmpegProcess Proc = sender as FFmpegProcess;
                long Frames = 0;
                for (int i = 0; i < hosts.Count; i++) {
                    if (hosts[i].LastStatusReceived != null)
                        Frames += hosts[i].LastStatusReceived.Frame;
                }
                WorkProgressBar.Value = FrameDone + Frames;
                PercentText.Text = (WorkProgressBar.Value / WorkProgressBar.Maximum).ToString("p1");
                SetPageTitle(PercentText.Text);
                // FpsText.Text = e.Status.Fps.ToString();

                // Time left will be updated only once per 2 updates to prevent changing too quickly
                EstimatedTimeLeftToggle = (EstimatedTimeLeftToggle + 1) % 2;
                if (EstimatedTimeLeftToggle == 0 && timeCalc != null) {
                    timeCalc.Calculate(Frames);
                    if (timeCalc.ResultTimeLeft > TimeSpan.Zero) {
                        TimeLeftText.Text = timeCalc.ResultTimeLeft.ToString(timeCalc.ResultTimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss");
                        FpsText.Text = timeCalc.ResultFps.ToString("0.0");
                    }
                }
            });
        }

        private void FFmpeg_Completed(object sender, FFmpeg.CompletedEventArgs e) {
            if (manager.AppExited)
                return;
            Dispatcher.Invoke(() => {
                FFmpegProcess Proc = sender as FFmpegProcess;
                if (e.Status == CompletionStatus.Error && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2yuv.exe"))
                    FFmpegErrorWindow.Instance(Owner, Proc);
                // hosts.Remove(Proc);
                if (Proc == task) {
                    task = null;
                    TaskStatusText.Text = "";
                }
                if (autoClose && !HasRunningHosts) {
                    ClosingState = 2;
                    this.Close();
                }
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (ClosingState == 0) {
                ClosingState = HasRunningHosts ? 1 : 2;
                autoClose = true;
                CancelButton.Content = "Cancelling...";
                CancelButton.IsEnabled = false;
                task?.Cancel();
                foreach (FFmpegProcess host in hosts) {
                    host.Cancel();
                }
            }
            if (ClosingState == 1)
                e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e) {
            manager.RaiseJobClosed(this, jobId);
        }

        private bool HasRunningHosts {
            get {
                return hosts.Any(h => h.LastCompletionStatus == CompletionStatus.None) ||
                    (task != null && task.LastCompletionStatus == CompletionStatus.None);
            }
        }
    }
}
