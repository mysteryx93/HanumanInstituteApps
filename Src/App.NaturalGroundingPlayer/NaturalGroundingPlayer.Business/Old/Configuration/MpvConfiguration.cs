using System;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.Services;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the configuration of MPC-HC.
    /// </summary>
    public class MpvConfiguration : IMpvConfiguration
    {
        private readonly IRegistryService _registry;
        private readonly IFileSystemService _fileSystem;
        private readonly IProcessService _process;
        private readonly IAppSettingsProvider _settings;

        public MpvConfiguration(IRegistryService registryService, IFileSystemService fileSystemService, IProcessService processService, IAppSettingsProvider settings)
        {
            _registry = registryService;
            _fileSystem = fileSystemService;
            _process = processService;
            _settings = settings;
        }

        /// <summary>
        /// Gets or sets whether MadVR is enabled in MPC-HC.
        /// </summary>
        public bool IsMadvrEnabled
        {
            get => _registry.MpcVideoRenderer == 12;
            set
            {
                if (value != IsMadvrEnabled && _settings.Value.MpcPath.Length > 0)
                {
                    KillMpcProcesses();
                    _registry.MpcVideoRenderer = value ? 12 : 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to play videos in loop.
        /// </summary>
        public bool Loop
        {
            get => _registry.MpcLoop;
            set
            {
                if (value != Loop && _settings.Value.MpcPath.Length > 0)
                {
                    KillMpcProcesses();
                    _registry.MpcLoop = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the player should remember file position when re-opening the player.
        /// </summary>
        public bool RememberFilePos
        {
            get => _registry.MpcRememberFilePos;
            set
            {
                if (value != RememberFilePos && _settings.Value.MpcPath.Length > 0)
                {
                    KillMpcProcesses();
                    _registry.MpcRememberFilePos = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to override aspect ratio as 16:9 (widescreen).
        /// </summary>
        public bool IsWidescreenEnabled
        {
            get => _registry.MpcAspectRatioX > 0;
            set
            {
                if (value != IsWidescreenEnabled && _settings.Value.MpcPath.Length > 0)
                {
                    KillMpcProcesses();
                    _registry.MpcAspectRatioX = value ? 16 : 0;
                    _registry.MpcAspectRatioY = value ? 9 : 0;
                }
            }
        }

        /// <summary>
        /// Kills any MPC-HC running instance.
        /// </summary>
        public void KillMpcProcesses()
        {
            var appName = _fileSystem.Path.GetFileNameWithoutExtension(_settings.Value.MpcPath);
            foreach (IProcess item in _process.GetProcessesByName(appName))
            {
                try
                {
                    item.Kill();
                }
                catch { }
            }
        }

        /// <summary>
        /// Starts a MPC process without hooking into it.
        /// </summary>
        /// <param name="fileName">The video file to open.</param>
        public void StartMpc(string fileName)
        {
            if (!string.IsNullOrEmpty(_settings.Value.MpcPath) && _fileSystem.File.Exists(_settings.Value.MpcPath))
            {
                _process.Start(_settings.Value.MpcPath, "\"" + fileName + "\"");
            }
        }
    }
}
