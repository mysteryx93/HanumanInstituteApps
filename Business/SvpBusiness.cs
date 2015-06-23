//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using DataAccess;

//namespace Business {
//    public class SvpBusiness {
//        public bool IsSvpEnabled {
//            get {
//                Process SvpProcess = GetSvpProcess();
//                return (SvpProcess != null);
//            }
//            set {
//                if (value != IsSvpEnabled) {
//                    if (value)
//                        Start();
//                    else
//                        Stop();
//                }
//            }
//        }

//        public void AutoConfigure(SvpStatus videoStatus) {
//            if (Settings.SavedFile.EnableSvp) {
//                bool DisableRes = (videoStatus == SvpStatus.QualityProblem) || (videoStatus == SvpStatus.PerformanceProblem && System.Windows.SystemParameters.PrimaryScreenHeight >= 1080);
//                if (videoStatus != SvpStatus.OK && DisableRes && IsSvpEnabled)
//                    Stop();
//                else if ((videoStatus == SvpStatus.OK || !DisableRes) && !IsSvpEnabled)
//                    Start();
//            } else if (IsSvpEnabled)
//                Stop();
//        }

//        private void Start() {
//            Process SvpProcess = GetSvpProcess();
//            if (SvpProcess == null && !string.IsNullOrEmpty(Settings.SavedFile.SvpPath)) {
//                ClearLog();
//                SvpProcess = new Process();
//                SvpProcess.StartInfo.FileName = Settings.SavedFile.SvpPath;
//                SvpProcess.Start();
//            }
//        }

//        private void Stop() {
//            Process SvpProcess = GetSvpProcess();
//            if (SvpProcess != null) {
//                SvpProcess.Kill();
//                MpcConfigBusiness.KillMpcProcesses();
//            }
//        }

//        public void ClearLog() {
//            string LogFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\SVP 3.1\Logs\Log.txt";
//            if (File.Exists(LogFile))
//                File.Delete(LogFile);
//        }

//        private Process GetSvpProcess() {
//            string AppName = Path.GetFileNameWithoutExtension(Settings.SavedFile.SvpPath);
//            return Process.GetProcessesByName(AppName).FirstOrDefault();
//        }
//    }
//}
