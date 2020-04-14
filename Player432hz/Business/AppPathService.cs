using System;
using System.Collections.Generic;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.Player432hz.Business {

    #region Interface

    public interface IAppPathService {
        /// <summary>
        /// Returns all valid video extensions.
        /// </summary>
        IList<string> VideoExtensions { get; }
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        IList<string> AudioExtensions { get; }
        /// <summary>
        /// Returns all valid image extensions.
        /// </summary>
        IList<string> ImageExtensions { get; }
        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        string UnhandledExceptionLogPath { get; }
        /// <summary>
        /// Returns the path where the 432hz Player is storing its Avisynth script during playback.
        /// </summary>
        string Player432hzScriptFile { get; }
        /// <summary>
        /// Returns the path where the 432hz Player settings file is stored.
        /// </summary>
        string Player432hzConfigFile { get; }
        /// <summary>
        /// Returns the relative path to access the temp folder within the Natural Grounding folder.
        /// </summary>
        string LocalTempPath { get; }
        /// <summary>
        /// Returns the system temp folder.
        /// </summary>
        string SystemTempPath { get; }
    }

    #endregion

    /// <summary>
    /// Manages the file system paths used by the application.
    /// </summary>
    public class AppPathService : IAppPathService {

        #region Declarations / Constructors

        private readonly IEnvironmentService environment;
        private readonly IFileSystemService fileSystem;

        public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService) {
            environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Returns all valid video extensions.
        /// </summary>
        public IList<string> VideoExtensions => videoExtensions ?? (videoExtensions = new List<string> { ".mp4", ".webm", ".avi", ".flv", ".mpg", ".mkv", ".wmv", ".tp", ".ts", ".mov", ".avs", ".m2v", ".vob" });
        private IList<string> videoExtensions;
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        public IList<string> AudioExtensions => audioExtensions ?? (audioExtensions = new List<string> { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" });
        private IList<string> audioExtensions;
        /// <summary>
        /// Returns all valid image extensions.
        /// </summary>
        public IList<string> ImageExtensions => imageExtensions ?? (imageExtensions = new List<string> { ".gif", ".jpg", ".png", ".bmp", ".tiff" });
        private IList<string> imageExtensions;

        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        public string UnhandledExceptionLogPath => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\Log.txt");
        /// <summary>
        /// Returns the path where the 432hz Player is storing its Avisynth script during playback.
        /// </summary>
        public string Player432hzScriptFile => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzPlaying.avs");
        /// <summary>
        /// Returns the path where the 432hz Player settings file is stored.
        /// </summary>
        public string Player432hzConfigFile => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzConfig.xml");

        /// <summary>
        /// Returns the relative path to access the temp folder within the Natural Grounding folder.
        /// </summary>
        public string LocalTempPath => @"Temp\";
        /// <summary>
        /// Returns the system temp folder.
        /// </summary>
        public string SystemTempPath => fileSystem.Path.GetTempPath();
        /// <summary>
        /// Returns the temp path for the media downloader.
        /// </summary>
    }
}
