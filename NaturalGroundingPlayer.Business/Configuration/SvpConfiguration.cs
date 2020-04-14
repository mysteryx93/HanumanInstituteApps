using System;
using System.Linq;
using System.Threading;
using EmergenceGuardian.CommonServices;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Manages the configuration of SVP (Smooth VIdeo Project).
    /// </summary>
    public interface ISvpConfiguration {
        /// <summary>
        /// Gets or sets whether SVP is enabled.
        /// </summary>
        bool IsSvpEnabled { get; set; }
        /// <summary>
        /// Clears SVP log files so that it doesn't display an error message when restarting.
        /// </summary>
        void ClearLog();
        /// <summary>
        /// Adds the Natural Grounding Player to SVP's blacklist so that it doesn't affect internal players.
        /// </summary>
        void AddAppToSvpBlacklist();
        /// <summary>
        /// Returns the SVP process.
        /// </summary>
        IProcess GetSvpProcess();
    }

    #endregion

    /// <summary>
    /// Manages the configuration of SVP (Smooth VIdeo Project).
    /// </summary>
    public class SvpConfiguration : ISvpConfiguration {

        #region Declarations / Constructors

        protected readonly IEnvironmentService environment;
        protected readonly IFileSystemService fileSystem;
        protected readonly IProcessService process;
        protected readonly ISettings settings;
        protected readonly IMpcConfiguration mpc;

        public SvpConfiguration() : this(new EnvironmentService(), new FileSystemService(), new ProcessService(), new Settings(), new MpcConfiguration()) { }

        public SvpConfiguration(IEnvironmentService environmentService, IFileSystemService fileSystemService, IProcessService processService, ISettings settings, IMpcConfiguration mpcConfig) {
            this.environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.process = processService ?? throw new ArgumentNullException(nameof(processService));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.mpc = mpcConfig ?? throw new ArgumentNullException(nameof(mpcConfig));
        }

        #endregion


        /// <summary>
        /// Gets or sets whether SVP is enabled.
        /// </summary>
        public bool IsSvpEnabled {
            get {
                IProcess SvpProcess = GetSvpProcess();
                return (SvpProcess != null);
            }
            set {
                if (value != IsSvpEnabled) {
                    if (value) {
                        // Starts the SVP process.
                        IProcess SvpProcess = GetSvpProcess();
                        if (SvpProcess == null && !string.IsNullOrEmpty(settings.Data.SvpPath)) {
                            ClearLog();
                            process.Start(settings.Data.SvpPath);
                        }
                    } else {
                        // Stops the SVP process.
                        IProcess SvpProcess = GetSvpProcess();
                        if (SvpProcess != null) {
                            SvpProcess.Kill();
                            mpc.KillMpcProcesses();
                        }
                    }
                    Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// Clears SVP log files so that it doesn't display an error message when restarting.
        /// </summary>
        public void ClearLog() {
            string LogFile = fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"\SVP 3.1\Logs\Log.txt");
            fileSystem.DeleteFileSilent(LogFile);
        }

        /// <summary>
        /// Adds the Natural Grounding Player to SVP's blacklist so that it doesn't affect internal players.
        /// </summary>
        public void AddAppToSvpBlacklist() {
            string AppName = environment.AppFriendlyName;
            string SettingFile = fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"\SVP 3.1\Settings\SVPMgr.ini");
            if (fileSystem.File.Exists(SettingFile)) {
                bool HasChanges = false;
                String[] EditFile = fileSystem.File.ReadAllLines(SettingFile).Select(line => {
                    if (line.StartsWith("BlackListApps=") && !line.Contains(AppName)) {
                        HasChanges = true;
                        return (line + ";" + AppName).Replace("=;", "=").Replace(";;", ";"); // Avoid corrupting the line if blacklist was empty.
                    } else
                        return line;
                }).ToArray();
                if (HasChanges) {
                    fileSystem.File.WriteAllLines(SettingFile, EditFile);
                    if (IsSvpEnabled) {
                        IsSvpEnabled = false;
                        IsSvpEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the SVP process.
        /// </summary>
        public IProcess GetSvpProcess() {
            string AppName = fileSystem.Path.GetFileNameWithoutExtension(settings.Data.SvpPath);
            return process.GetProcessesByName(AppName).FirstOrDefault();
        }
    }
}
