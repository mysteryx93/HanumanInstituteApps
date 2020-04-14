using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading;
using HanumanInstitute.Encoder;
using System.Threading.Tasks;

namespace HanumanInstitute.CommonWpfApp {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegWindow : Window, IUserInterfaceWindow {
        public static FFmpegWindow Instance(Window parent, string title, bool autoClose, FFmpegUserInterfaceManager manager) {
            FFmpegWindow F = new FFmpegWindow();
            F.Owner = parent;
            F.title = title;
            F.autoClose = autoClose;
            F.manager = manager;
            F.Show();
            return F;
        }

        private List<IProcessWorker> hosts = new List<IProcessWorker>();
        private IProcessWorker task;
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

        public void DisplayTask(IProcessWorker taskArg) {
            if (manager.AppExited)
                return;
            Dispatcher.Invoke(() => {
                jobId = taskArg.Options.JobId;
                if (taskArg.Options.IsMainTask) {
                    hosts.Add(taskArg);
                    var taskEncoder = taskArg as IProcessWorkerEncoder;
                    if (taskEncoder != null)
                    {
                        var hostsEncoder = hosts.OfType<IProcessWorkerEncoder>();
                        FrameTotal = taskEncoder.Options.TotalFrameCount > 0 ?
                            taskEncoder.Options.TotalFrameCount : hostsEncoder.Max(h => h.Options.FrameCount + h.Options.ResumePos);
                        FrameTodo = hostsEncoder.Sum(h => h.Options.FrameCount);
                        if (taskEncoder.Options.TotalFrameCount > 0)
                            FrameTodo += FrameTotal - hostsEncoder.Max(h => h.Options.FrameCount + h.Options.ResumePos);
                        FrameDone = FrameTotal - FrameTodo;
                        WorkProgressBar.Maximum = FrameTodo > 0 ? FrameTotal : 1;
                        timeCalc.FrameCount = FrameTodo;
                        taskEncoder.FileInfoUpdated += FFmpeg_InfoUpdated;
                        taskEncoder.ProgressReceived += TaskEncoder_ProgressReceived; ;
                    }

                    taskArg.ProcessCompleted += FFmpeg_ProcessCompleted;
                    //if (taskArg.Options.ResumePos > 0)
                    //    PercentText.Text = (taskArg.Options.ResumePos / WorkProgressBar.Maximum).ToString("p1");
                    //else
                    if (PercentText.Text.Length == 0) {
                        PercentText.Text = 0.ToString("p1");
                        SetPageTitle(PercentText.Text);
                    }
                } else {
                    task = taskArg;
                    task.ProcessCompleted += FFmpeg_ProcessCompleted; ;
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
                    var hostsEncoder = hosts.OfType<IProcessWorkerEncoder>();
                    FrameTotal = hostsEncoder.Max(h => h.Options.FrameCount + h.Options.ResumePos);
                    FrameTodo = hostsEncoder.Sum(h => h.Options.FrameCount);
                    FrameDone = FrameTotal - FrameTodo;

                    WorkProgressBar.Maximum = FrameTotal;
                    timeCalc.FrameCount = FrameTotal;
                }
            });
        }

        private int EstimatedTimeLeftToggle = 0;

        private void TaskEncoder_ProgressReceived(object sender, ProgressReceivedEventArgs e)
        {
            if (manager.AppExited)
                return;
            Dispatcher.Invoke(() => {
                IProcessWorker Proc = sender as IProcessWorker;
                long Frames = hosts.OfType<IProcessWorkerEncoder>()
                    .Select(h => h.LastProgressReceived)
                    .OfType<ProgressStatusFFmpeg>()
                    .Sum(s => s.Frame);
                WorkProgressBar.Value = FrameDone + Frames;
                PercentText.Text = (WorkProgressBar.Value / WorkProgressBar.Maximum).ToString("p1");
                SetPageTitle(PercentText.Text);
                // FpsText.Text = e.Status.Fps.ToString();

                // Time left will be updated only once per 2 updates to prevent changing too quickly
                EstimatedTimeLeftToggle = (EstimatedTimeLeftToggle + 1) % 2;
                if (EstimatedTimeLeftToggle == 0 && timeCalc != null)
                {
                    timeCalc.Calculate(Frames);
                    if (timeCalc.ResultTimeLeft > TimeSpan.Zero)
                    {
                        TimeLeftText.Text = timeCalc.ResultTimeLeft.ToString(timeCalc.ResultTimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss");
                        FpsText.Text = timeCalc.ResultFps.ToString("0.0");
                    }
                }
            });
        }

        private void FFmpeg_ProcessCompleted(object sender, ProcessCompletedEventArgs e)
        {
            if (manager.AppExited)
                return;
            Dispatcher.Invoke(() => {
                IProcessWorker Proc = sender as IProcessWorker;
                if (e.Status == CompletionStatus.Failed && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2pipemod.exe"))
                    FFmpegErrorWindow.Instance(Owner, Proc);
                // hosts.Remove(Proc);
                if (Proc == task)
                {
                    task = null;
                    TaskStatusText.Text = "";
                }
                if (autoClose && !HasRunningHosts)
                {
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
                foreach (IProcessWorker host in hosts) {
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
