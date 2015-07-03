using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using Microsoft.Win32;

namespace Business {
    public static class MpcConfigBusiness {
        /// <summary>
        /// Configures MPC-HC settings to work properly with the Natural Grounding Player.
        /// </summary>
        public static void ConfigureSettings() {
            Loop = true;
            RememberFilePos = false;
            AddToSvpBlacklist();
        }

        /// <summary>
        /// Returns the path of MPC-HC's executable file.
        /// </summary>
        /// <returns>The path to MPC-HC.exe, or null if not found.</returns>
        public static string MpcPath() {
            if (Settings.SavedFile != null)
                return MpcPath(Settings.SavedFile.SvpPath);
            else 
                return MpcPath(null);
        }

        /// <summary>
        /// Returns the path of MPC-HC's executable file.
        /// </summary>
        /// <param name="svpPath">The path to SVP to help find MPC-HC if its registry key is missing.</param>
        /// <returns>The path to MPC-HC.exe, or null if not found.</returns>
        public static string MpcPath(string svpPath) {
            string Result = Registry.GetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\", "ExePath", "") as string;
            if ((string.IsNullOrEmpty(Result) || !File.Exists(Result)) && !string.IsNullOrEmpty(svpPath))
                Result = Path.Combine(Path.GetDirectoryName(svpPath), @"MPC-HC\mpc-hc.exe");
            // Ensure file exists.
            if (!string.IsNullOrEmpty(Result) && File.Exists(Result))
                return Result;
            else
                return null;
        }

        /// <summary>
        /// Gets or sets whether MadVR is enabled in MPC-HC.
        /// </summary>
        public static bool IsMadvrEnabled {
            get {
                return (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "DSVidRen", 0) == 12;
            }
            set {
                if (value != IsMadvrEnabled && MpcPath() != null) {
                    KillMpcProcesses();
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "DSVidRen", value ? 12 : 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to play videos in loop.
        /// </summary>
        private static bool Loop {
            get {
                return (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "Loop", 0) > 0;
            }
            set {
                if (value != Loop && MpcPath() != null) {
                    KillMpcProcesses();
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "Loop", value ? 1 : 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the player should remember file position when re-opening the player.
        /// </summary>
        private static bool RememberFilePos {
            get {
                return (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "RememberFilePos", 0) > 0;
            }
            set {
                if (value != RememberFilePos && MpcPath() != null) {
                    KillMpcProcesses();
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "RememberFilePos", value ? 1 : 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to override aspect ratio as 16:9 (widescreen).
        /// </summary>
        public static bool IsWidescreenEnabled {
            get {
                return (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "AspectRatioX", 0) > 0;
            }
            set {
                if (value != IsWidescreenEnabled && MpcPath() != null) {
                    KillMpcProcesses();
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "AspectRatioX", value ? 16 : 0);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\", "AspectRatioY", value ? 9 : 0);
                }
            }
        }

        /// <summary>
        /// Kills any MPC-HC running instance.
        /// </summary>
        public static void KillMpcProcesses() {
            string AppName = Path.GetFileNameWithoutExtension(MpcConfigBusiness.MpcPath());
            foreach (Process item in Process.GetProcessesByName(AppName)) {
                try {
                    item.Kill();
                } catch { }
            }
        }

        /// <summary>
        /// Gets or sets whether SVP is enabled.
        /// </summary>
        public static bool IsSvpEnabled {
            get {
                Process SvpProcess = GetSvpProcess();
                return (SvpProcess != null);
            }
            set {
                if (value != IsSvpEnabled) {
                    if (value) {
                        // Starts the SVP process.
                        Process SvpProcess = GetSvpProcess();
                        if (SvpProcess == null && !string.IsNullOrEmpty(Settings.SavedFile.SvpPath)) {
                            ClearLog();
                            SvpProcess = new Process();
                            SvpProcess.StartInfo.FileName = Settings.SavedFile.SvpPath;
                            SvpProcess.Start();
                        }
                    } else {
                        // Stops the SVP process.
                        Process SvpProcess = GetSvpProcess();
                        if (SvpProcess != null) {
                            SvpProcess.Kill();
                            MpcConfigBusiness.KillMpcProcesses();
                        }
                    }
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// Automatically starts or stops SVP and madVR based on current video requirements.
        /// </summary>
        /// <param name="videoStatus">The media containing performance status information.</param>
        public static void AutoConfigure(Media videoStatus) {
            // If no status information is supplied, create default object with default values of 'false'.
            if (videoStatus == null)
                videoStatus = new Media();

            IsSvpEnabled = Settings.SavedFile.EnableSvp && !videoStatus.DisableSvp;
            IsMadvrEnabled = Settings.SavedFile.EnableMadVR && !videoStatus.DisableMadVr;
        }

        /// <summary>
        /// Clears SVP log files so that it doesn't display an error message when restarting.
        /// </summary>
        public static void ClearLog() {
            string LogFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\SVP 3.1\Logs\Log.txt";
            if (File.Exists(LogFile))
                File.Delete(LogFile);
        }

        /// <summary>
        /// Adds the Natural Grounding Player to SVP's blacklist so that it doesn't affect internal players.
        /// </summary>
        public static void AddToSvpBlacklist() {
            string AppName = System.AppDomain.CurrentDomain.FriendlyName;
            string SettingFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\SVP 3.1\Settings\SVPMgr.ini";
            if (File.Exists(SettingFile)) {
                bool HasChanges = false;
                String[] EditFile = File.ReadAllLines(SettingFile).Select(line => {
                    if (line.StartsWith("BlackListApps=") && !line.Contains(AppName)) {
                        HasChanges = true;
                        return (line + ";" + AppName).Replace("=;", "=").Replace(";;", ";"); // Avoid corrupting the line if blacklist was empty.
                    } else
                        return line;
                }).ToArray();
                if (HasChanges) {
                    File.WriteAllLines(SettingFile, EditFile);
                    if (IsSvpEnabled) {
                        IsSvpEnabled = false;
                        IsSvpEnabled = true;
                    }
                }
            }
        }

        private static string FixBlackListLine(string line) {
            return line;
        }

        /// <summary>
        /// Returns the SVP process.
        /// </summary>
        private static Process GetSvpProcess() {
            string AppName = Path.GetFileNameWithoutExtension(Settings.SavedFile.SvpPath);
            return Process.GetProcessesByName(AppName).FirstOrDefault();
        }
    }
}
