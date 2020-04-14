using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Management;
using HanumanInstitute.Encoder;
using System.Diagnostics;

namespace HanumanInstitute.CommonWpfApp {
    public class FFmpegUserInterfaceManager : UserInterfaceManagerBase {
        private Window parent;

        public FFmpegUserInterfaceManager(Window parent) {
            this.parent = parent;
        }

        public override IUserInterfaceWindow CreateUI(string title, bool autoClose) {
            return Application.Current.Dispatcher.Invoke(() => FFmpegWindow.Instance(parent, title, autoClose, this));
        }

        public override void DisplayError(IProcessWorker host) {
            Application.Current.Dispatcher.Invoke(() => FFmpegErrorWindow.Instance(parent, host));
        }

        /// <summary>
        /// Occurs when a job is finished closing.
        /// </summary>
        public EventHandler<JobEventArgs> JobClosed;

        /// <summary>
        /// Raises the JobClosed event.
        /// </summary>
        public void RaiseJobClosed(object sender, object jobId) {
            JobClosed?.Invoke(sender, new JobEventArgs(jobId));
        }


        /// <summary>
        /// Occurs when a FFMPEG process needs to be killed. When piping avs2yuv to ffmpeg, hard-kill avs2yuv 
        /// instead of soft killing the process. FFmpeg will then cleanly terminate on its own.
        /// </summary>
        //public static void CloseProcess(object sender, ProcessEventArgs e) {
        //    if (e.Process.ProcessName == "cmd")
        //        KillAvs2yuv(e.Process.Id);
        //    // Even if we hard kill, we will also soft kill 'cmd' to wait until process terminates
        //    // e.Handled remains false
        //}

        /// <summary>
        /// Kills the sub-process avs2yuv and returns whether such a process was found and killed.
        /// </summary>
        /// <param name="parentId">The parent process, generally 'cmd'</param>
        /// <returns>Whether a sub-process called avs2yuv was found and killed.</returns>
        private static bool KillAvs2yuv(int parentId) {
            var query = "Select * From Win32_Process Where ParentProcessId = " + parentId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();
            foreach (var proc in processList) {
                if (proc.GetPropertyValue("Name").ToString().ToLower() == "avs2pipemod.exe") {
                    try {
                        Process P = Process.GetProcessById(Convert.ToInt32(proc.GetPropertyValue("ProcessId")));
                        if (P != null) {
                            P.Kill();
                            return true;
                        }
                    } catch {
                        return false;
                    }
                }
            }
            return false;
        }
    }

    public class JobEventArgs : EventArgs {
        public object JobId { get; set; }

        public JobEventArgs() { }

        public JobEventArgs(object jobId) {
            this.JobId = jobId;
        }
    }
}
