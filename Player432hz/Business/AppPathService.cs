using System;
using System.Collections.Generic;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.Player432hz.Business
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
            _environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        /// <summary>
        /// Returns all valid video extensions.
        /// </summary>
        public IList<string> VideoExtensions => _videoExtensions ?? (_videoExtensions = new List<string> { ".mp4", ".webm", ".avi", ".flv", ".mpg", ".mkv", ".wmv", ".tp", ".ts", ".mov", ".avs", ".m2v", ".vob" });
        private IList<string>? _videoExtensions;
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        public IList<string> AudioExtensions => _audioExtensions ?? (_audioExtensions = new List<string> { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" });
        private IList<string>? _audioExtensions;
        /// <summary>
        /// Returns all valid image extensions.
        /// </summary>
        public IList<string> ImageExtensions => _imageExtensions ?? (_imageExtensions = new List<string> { ".gif", ".jpg", ".png", ".bmp", ".tiff" });
        private IList<string>? _imageExtensions;

        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        public string UnhandledExceptionLogPath => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\Log.txt");
        /// <summary>
        /// Returns the path where the 432hz Player is storing its Avisynth script during playback.
        /// </summary>
        public string Player432hzScriptFile => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzPlaying.avs");
        /// <summary>
        /// Returns the path where the 432hz Player settings file is stored.
        /// </summary>
        public string Player432hzConfigFile => _fileSystem.Path.Combine(_environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzConfig.xml");

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
    }
}
