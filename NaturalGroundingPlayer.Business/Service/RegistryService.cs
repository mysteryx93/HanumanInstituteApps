using System;
using System.IO;
using Microsoft.Win32;
using EmergenceGuardian.CommonServices;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Provides access to Windows registry keys.
    /// </summary>
    public interface IRegistryService {
        /// <summary>
        /// Gets the install location of MPC-HC from the registry, validates the file exists, and returns a default location if not found.
        /// </summary>
        string MpcExePath { get; }
        /// <summary>
        /// Gets or sets MPC-HC's Video Renderer.
        /// </summary>
        int? MpcVideoRenderer { get; set; }
        /// <summary>
        /// Gets or sets whether MPC-HC has Loop enabled.
        /// </summary>
        bool MpcLoop { get; set; }
        /// <summary>
        /// Gets or sets whether MPC-HC remembers last file position after closing.
        /// </summary>
        bool MpcRememberFilePos { get; set; }
        /// <summary>
        /// Gets or sets MPC-HC aspect ratio X.
        /// </summary>
        int? MpcAspectRatioX { get; set; }
        /// <summary>
        /// Gets or sets MPC-HC aspect ratio Y.
        /// </summary>
        int? MpcAspectRatioY { get; set; }
    }

    #endregion

    /// <summary>
    /// Provides access to Windows registry keys.
    /// </summary>
    public class RegistryService : IRegistryService {

        #region Declarations / Constructors

        private const string MpcKey = @"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\";
        private const string MpcSettingsKey = @"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\";
        protected readonly IEnvironmentService environment;
        protected readonly IFileSystemService fileSystem;

        public RegistryService() { }

        public RegistryService(IEnvironmentService environmentService, IFileSystemService fileSystemService) {
            this.environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Gets the install location of MPC-HC from the registry, validates the file exists, and returns a default location if not found.
        /// </summary>
        public string MpcExePath {
            get {
                string Result = Registry.GetValue(MpcKey, "ExePath", "") as string;
                if ((string.IsNullOrEmpty(Result) || !fileSystem.File.Exists(Result)))
                    Result = Path.Combine(environment.ProgramFilesX86, @"MPC-HC\mpc-hc.exe");
                return Result;
            }
        }

        /// <summary>
        /// Gets or sets MPC-HC's Video Renderer.
        /// </summary>
        public int? MpcVideoRenderer {
            get => (int?)Registry.GetValue(MpcSettingsKey, "DSVidRen", 0);
            set => Registry.SetValue(MpcSettingsKey, "DSVidRen", value);
        }

        /// <summary>
        /// Gets or sets whether MPC-HC has Loop enabled.
        /// </summary>
        public bool MpcLoop {
            get => (int?)Registry.GetValue(MpcSettingsKey, "Loop", 0) > 0;
            set => Registry.SetValue(MpcSettingsKey, "Loop", value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets whether MPC-HC remembers last file position after closing.
        /// </summary>
        public bool MpcRememberFilePos {
            get => (int?)Registry.GetValue(MpcSettingsKey, "RememberFilePos", 0) > 0;
            set => Registry.SetValue(MpcSettingsKey, "RememberFilePos", value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets MPC-HC aspect ratio X.
        /// </summary>
        public int? MpcAspectRatioX {
            get => (int?)Registry.GetValue(MpcSettingsKey, "AspectRatioX", 0);
            set => Registry.SetValue(MpcSettingsKey, "AspectRatioX", value);
        }

        /// <summary>
        /// Gets or sets MPC-HC aspect ratio Y.
        /// </summary>
        public int? MpcAspectRatioY {
            get => (int?)Registry.GetValue(MpcSettingsKey, "AspectRatioY", 0);
            set => Registry.SetValue(MpcSettingsKey, "AspectRatioY", value);
        }
    }
}
