using System;
using EmergenceGuardian.CommonServices;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Manages the configuration of MPC-HC.
    /// </summary>
    public interface IMpcConfiguration {
        /// <summary>
        /// Gets or sets whether MadVR is enabled in MPC-HC.
        /// </summary>
        bool IsMadvrEnabled { get; set; }
        /// <summary>
        /// Gets or sets whether to play videos in loop.
        /// </summary>
        bool Loop { get; set; }
        /// <summary>
        /// Gets or sets whether the player should remember file position when re-opening the player.
        /// </summary>
        bool RememberFilePos { get; set; }
        /// <summary>
        /// Gets or sets whether to override aspect ratio as 16:9 (widescreen).
        /// </summary>
        bool IsWidescreenEnabled { get; set; }
        /// <summary>
        /// Kills any MPC-HC running instance.
        /// </summary>
        void KillMpcProcesses();
        /// <summary>
        /// Starts a MPC process without hooking into it.
        /// </summary>
        /// <param name="fileName">The video file to open.</param>
        void StartMpc(string fileName);
    }

    #endregion

    /// <summary>
    /// Manages the configuration of MPC-HC.
    /// </summary>
    public class MpcConfiguration : IMpcConfiguration {

        #region Declarations / Constructors

        protected readonly IRegistryService registry;
        protected readonly IFileSystemService fileSystem;
        protected readonly IProcessService process;
        protected readonly ISettings settings;

        public MpcConfiguration() : this(new RegistryService(), new FileSystemService(), new ProcessService(), new Settings()) { }

        public MpcConfiguration(IRegistryService registryService, IFileSystemService fileSystemService, IProcessService processService, ISettings settings) {
            this.registry = registryService ?? throw new ArgumentNullException(nameof(registryService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.process = processService ?? throw new ArgumentNullException(nameof(processService));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        #endregion

        /// <summary>
        /// Gets or sets whether MadVR is enabled in MPC-HC.
        /// </summary>
        public bool IsMadvrEnabled {
            get => registry.MpcVideoRenderer == 12;
            set {
                if (value != IsMadvrEnabled && settings.Data.MpcPath.Length > 0) {
                    KillMpcProcesses();
                    registry.MpcVideoRenderer = value ? 12 : 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to play videos in loop.
        /// </summary>
        public bool Loop {
            get => registry.MpcLoop;
            set {
                if (value != Loop && settings.Data.MpcPath.Length > 0) {
                    KillMpcProcesses();
                    registry.MpcLoop = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the player should remember file position when re-opening the player.
        /// </summary>
        public bool RememberFilePos {
            get => registry.MpcRememberFilePos;
            set {
                if (value != RememberFilePos && settings.Data.MpcPath.Length > 0) {
                    KillMpcProcesses();
                    registry.MpcRememberFilePos = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to override aspect ratio as 16:9 (widescreen).
        /// </summary>
        public bool IsWidescreenEnabled {
            get => registry.MpcAspectRatioX > 0;
            set {
                if (value != IsWidescreenEnabled && settings.Data.MpcPath.Length > 0) {
                    KillMpcProcesses();
                    registry.MpcAspectRatioX = value ? 16 : 0;
                    registry.MpcAspectRatioY = value ? 9 : 0;
                }
            }
        }

        /// <summary>
        /// Kills any MPC-HC running instance.
        /// </summary>
        public void KillMpcProcesses() {
            string AppName = fileSystem.Path.GetFileNameWithoutExtension(settings.Data.MpcPath);
            foreach (IProcess item in process.GetProcessesByName(AppName)) {
                try {
                    item.Kill();
                } catch { }
            }
        }

        /// <summary>
        /// Starts a MPC process without hooking into it.
        /// </summary>
        /// <param name="fileName">The video file to open.</param>
        public void StartMpc(string fileName) {
            if (!string.IsNullOrEmpty(settings.Data.MpcPath) && fileSystem.File.Exists(settings.Data.MpcPath))
                process.Start(settings.Data.MpcPath, "\"" + fileName + "\"");
        }
    }
}
