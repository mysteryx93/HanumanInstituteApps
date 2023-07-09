using System;
using System.Collections.Generic;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Manages the file system paths used by the application.
    /// </summary>
    public class AppPathService : IAppPathService
    {
        private readonly IEnvironmentService _environment;
        private readonly IFileSystemService _fileSystem;

        public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService)
        {
            _environment = environmentService;
            _fileSystem = fileSystemService;
        }

        /// <summary>
        /// Returns all valid video extensions.
        /// </summary>
        public IEnumerable<string> VideoExtensions => _videoExtensions ??= new[] { ".mp4", ".webm", ".avi", ".flv", ".mpg", ".mkv", ".wmv", ".tp", ".ts", ".mov", ".avs", ".m2v", ".vob" };
        private IEnumerable<string>? _videoExtensions;
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        public IEnumerable<string> AudioExtensions => _audioExtensions ??= new[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" };
        private IEnumerable<string>? _audioExtensions;
        /// <summary>
        /// Returns all valid image extensions.
        /// </summary>
        public IEnumerable<string> ImageExtensions => _imageExtensions ??= new[] { ".gif", ".jpg", ".png", ".bmp", ".tiff" };
        private IEnumerable<string>? _imageExtensions;

        /// <summary>
        /// Returns all extensions for specified media type.
        /// </summary>
        /// <param name="mediaType">The type of media for which to get extensions.</param>
        /// <returns>A string array of file extensions.</returns>
        public IEnumerable<string> GetMediaTypeExtensions(MediaType mediaType)
        {
            if (mediaType == MediaType.Video)
            {
                return VideoExtensions;
            }
            else if (mediaType == MediaType.Audio)
            {
                return AudioExtensions;
            }
            else if (mediaType == MediaType.Image)
            {
                return ImageExtensions;
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Returns the path where the settings file is stored.
        /// </summary>
        public string SettingsPath => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\Settings.xml");
        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        public string UnhandledExceptionLogPath => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\Log.txt");
        /// <summary>
        /// Returns the path where the database file is stored.
        /// </summary>
        public string DatabasePath => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\NaturalGroundingVideos.db");
        /// <summary>
        /// Returns the path where the blank initial database is stored.
        /// </summary>
        public string InitialDatabasePath => _fileSystem.Path.Combine(_environment.AppDirectory, "InitialDatabase.db");
        /// <summary>
        /// Returns the path where AviSynth plugins are located.
        /// </summary>
        public string AvisynthPluginsPath => _fileSystem.Path.Combine(_environment.AppDirectory, @"Encoder\");
        /// <summary>
        /// Returns the path where the 432hz Player is storing its Avisynth script during playback.
        /// </summary>
        public string Player432hzScriptFile => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzPlaying.avs");
        /// <summary>
        /// Returns the path where the 432hz Player settings file is stored.
        /// </summary>
        public string Player432hzConfigFile => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzConfig.xml");
        /// <summary>
        /// Returns the path where the Powerlimnals Player settings file is stored.
        /// </summary>
        public string PowerliminalsPlayerConfigFile => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\PowerliminalsConfig.xml");

        /// <summary>
        /// Returns the relative path to access FFmpeg.
        /// </summary>
        public string FFmpegPath => @"Encoder\ffmpeg.exe";
        /// <summary>
        /// Returns the relative path to access the temp folder within the Natural Grounding folder.
        /// </summary>
        public string LocalTempPath => @"Temp\";
        /// <summary>
        /// Returns the system temp folder.
        /// </summary>
        public string SystemTempPath => _fileSystem.Path.GetTempPath();
        /// <summary>
        /// Returns the temp path for the media downloader.
        /// </summary>
        public string DownloaderTempPath => _fileSystem.Path.Combine(SystemTempPath, @"YangYoutubeDownloader\");
        //public string X264Path => @"Encoder\x264-10bit.exe";
        //public string Avs2yuvPath => @"Encoder\avs2yuv.exe";

        /// <summary>
        /// Returns the default Natural Grounding folder.
        /// </summary>
        public string DefaultNaturalGroundingFolder => _environment.SystemRootDirectory + "Natural Grounding\\";

        /// <summary>
        /// Auto-detects SVP path.
        /// </summary>
        /// <returns>The default SVP path.</returns>
        public string GetDefaultSvpPath()
        {
            var defaultPaths = new string[] {
                @"SVP 4\SVPManager.exe",
                @"SVP 4 Free\SVPManager.exe",
                @"SVP 4 Dev\SVPManager.exe",
                @"SVP\SVPMgr.exe"
            };
            var programFiles = _environment.ProgramFilesX86;
            string itemPath;
            foreach (var item in defaultPaths)
            {
                itemPath = _fileSystem.Path.Combine(programFiles, item);
                if (_fileSystem.File.Exists(itemPath))
                {
                    return itemPath;
                }
            }
            return _fileSystem.Path.Combine(programFiles, defaultPaths[0]);
        }
    }
}
