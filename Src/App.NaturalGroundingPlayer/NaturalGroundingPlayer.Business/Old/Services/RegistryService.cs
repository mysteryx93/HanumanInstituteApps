using System;
using System.IO;
using HanumanInstitute.CommonServices;
using Microsoft.Win32;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Provides access to Windows registry keys.
    /// </summary>
    public class RegistryService : IRegistryService
    {
        private const string MpcKey = @"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\";
        private const string MpcSettingsKey = @"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\";
        private readonly IEnvironmentService _environment;
        private readonly IFileSystemService _fileSystem;

        public RegistryService(IEnvironmentService environmentService, IFileSystemService fileSystemService)
        {
            _environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        /// <summary>
        /// Gets the install location of MPC-HC from the registry, validates the file exists, and returns a default location if not found.
        /// </summary>
        public string MpcExePath
        {
            get
            {
                var result = Registry.GetValue(MpcKey, "ExePath", "") as string;
                if ((string.IsNullOrEmpty(result) || !_fileSystem.File.Exists(result)))
                {
                    result = Path.Combine(_environment.ProgramFilesX86, @"MPC-HC\mpc-hc.exe");
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets MPC-HC's Video Renderer.
        /// </summary>
        public int? MpcVideoRenderer
        {
            get => (int?)Registry.GetValue(MpcSettingsKey, "DSVidRen", 0);
            set => Registry.SetValue(MpcSettingsKey, "DSVidRen", value);
        }

        /// <summary>
        /// Gets or sets whether MPC-HC has Loop enabled.
        /// </summary>
        public bool MpcLoop
        {
            get => (int?)Registry.GetValue(MpcSettingsKey, "Loop", 0) > 0;
            set => Registry.SetValue(MpcSettingsKey, "Loop", value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets whether MPC-HC remembers last file position after closing.
        /// </summary>
        public bool MpcRememberFilePos
        {
            get => (int?)Registry.GetValue(MpcSettingsKey, "RememberFilePos", 0) > 0;
            set => Registry.SetValue(MpcSettingsKey, "RememberFilePos", value ? 1 : 0);
        }

        /// <summary>
        /// Gets or sets MPC-HC aspect ratio X.
        /// </summary>
        public int? MpcAspectRatioX
        {
            get => (int?)Registry.GetValue(MpcSettingsKey, "AspectRatioX", 0);
            set => Registry.SetValue(MpcSettingsKey, "AspectRatioX", value);
        }

        /// <summary>
        /// Gets or sets MPC-HC aspect ratio Y.
        /// </summary>
        public int? MpcAspectRatioY
        {
            get => (int?)Registry.GetValue(MpcSettingsKey, "AspectRatioY", 0);
            set => Registry.SetValue(MpcSettingsKey, "AspectRatioY", value);
        }
    }
}
