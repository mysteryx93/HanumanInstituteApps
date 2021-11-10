using System;
using System.Linq;
using System.Threading;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.Services;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the configuration of SVP (Smooth Video Project).
    /// </summary>
    public class SvpConfiguration : ISvpConfiguration
    {
        private readonly IEnvironmentService _environment;
        private readonly IFileSystemService _fileSystem;
        private readonly IProcessService _process;
        private readonly IAppSettingsProvider _settings;
        // private readonly IMpcConfiguration mpc;

        public SvpConfiguration(IEnvironmentService environmentService, IFileSystemService fileSystemService, IProcessService processService, IAppSettingsProvider settings)
        {
            _environment = environmentService;
            _fileSystem = fileSystemService;
            _process = processService;
            _settings = settings;
        }

        /// <summary>
        /// Gets or sets whether SVP is enabled.
        /// </summary>
        public bool IsSvpEnabled
        {
            get => GetSvpProcess() != null;
            set
            {
                if (value != IsSvpEnabled)
                {
                    if (value)
                    {
                        // Starts the SVP process.
                        var svpProcess = GetSvpProcess();
                        if (svpProcess == null && !string.IsNullOrEmpty(_settings.Value.SvpPath))
                        {
                            ClearLog();
                            _process.Start(_settings.Value.SvpPath);
                        }
                    }
                    else
                    {
                        // Stops the SVP process.
                        var svpProcess = GetSvpProcess();
                        if (svpProcess != null)
                        {
                            svpProcess.Kill();
                            // mpc.KillMpcProcesses();
                        }
                    }
                    Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// Clears SVP log files so that it doesn't display an error message when restarting.
        /// </summary>
        public void ClearLog()
        {
            var logFile = _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"\SVP 3.1\Logs\Log.txt");
            _fileSystem.DeleteFileSilent(logFile);
        }

        /// <summary>
        /// Adds the Natural Grounding Player to SVP's blacklist so that it doesn't affect internal players.
        /// </summary>
        public void AddAppToSvpBlacklist()
        {
            var appName = _environment.AppFriendlyName;
            var settingFile = _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"\SVP 3.1\Settings\SVPMgr.ini");
            if (_fileSystem.File.Exists(settingFile))
            {
                var hasChanges = false;
                var editFile = _fileSystem.File.ReadAllLines(settingFile).Select(line =>
                {
                    if (line.StartsWith("BlackListApps=") && !line.Contains(appName))
                    {
                        hasChanges = true;
                        return (line + ";" + appName).Replace("=;", "=").Replace(";;", ";"); // Avoid corrupting the line if blacklist was empty.
                    }
                    else
                    {
                        return line;
                    }
                }).ToArray();
                if (hasChanges)
                {
                    _fileSystem.File.WriteAllLines(settingFile, editFile);
                    if (IsSvpEnabled)
                    {
                        IsSvpEnabled = false;
                        IsSvpEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the SVP process.
        /// </summary>
        public IProcess GetSvpProcess()
        {
            var appName = _fileSystem.Path.GetFileNameWithoutExtension(_settings.Value.SvpPath);
            return _process.GetProcessesByName(appName).FirstOrDefault();
        }
    }
}
