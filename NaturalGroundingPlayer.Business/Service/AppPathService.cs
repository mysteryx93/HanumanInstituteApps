using System;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    public interface IAppPathService {
        /// <summary>
        /// Returns all valid video extensions.
        /// </summary>
        string[] VideoExtensions { get; }
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        string[] AudioExtensions { get; }
        /// <summary>
        /// Returns all valid image extensions.
        /// </summary>
        string[] ImageExtensions { get; }
        /// <summary>
        /// Returns all extensions for specified media type.
        /// </summary>
        /// <param name="mediaType">The type of media for which to get extensions.</param>
        /// <returns>A string array of file extensions.</returns>
        string[] GetMediaTypeExtensions(MediaType mediaType);
        /// <summary>
        /// Returns the path where the settings file is stored.
        /// </summary>
        string SettingsPath { get; }
        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        string UnhandledExceptionLogPath { get; }
        /// <summary>
        /// Returns the path where the database file is stored.
        /// </summary>
        string DatabasePath { get; }
        /// <summary>
        /// Returns the path where the blank initial database is stored.
        /// </summary>
        string InitialDatabasePath { get; }
        /// <summary>
        /// Returns the path where AviSynth plugins are located.
        /// </summary>
        string AvisynthPluginsPath { get; }
        /// <summary>
        /// Returns the path where the 432hz Player is storing its Avisynth script during playback.
        /// </summary>
        string Player432hzScriptFile { get; }
        /// <summary>
        /// Returns the path where the 432hz Player settings file is stored.
        /// </summary>
        string Player432hzConfigFile { get; }
        /// <summary>
        /// Returns the path where the Powerlimnals Player settings file is stored.
        /// </summary>
        string PowerliminalsPlayerConfigFile { get; }
        /// <summary>
        /// Returns the relative path to access FFmpeg.
        /// </summary>
        string FFmpegPath { get; }
        /// <summary>
        /// Returns the relative path to access the temp folder within the Natural Grounding folder.
        /// </summary>
        string LocalTempPath { get; }
        /// <summary>
        /// Returns the system temp folder.
        /// </summary>
        string SystemTempPath { get; }
        /// <summary>
        /// Returns the temp path for the media downloader.
        /// </summary>
        string DownloaderTempPath { get; }
        /// <summary>
        /// Returns the default Natural Grounding folder.
        /// </summary>
        string DefaultNaturalGroundingFolder { get; }
        /// <summary>
        /// Auto-detects SVP path.
        /// </summary>
        /// <returns>The default SVP path.</returns>
        string GetDefaultSvpPath();
        // void WriteLogFile(string content);
    }

    #endregion

    /// <summary>
    /// Manages the file system paths used by the application.
    /// </summary>
    public class AppPathService : IAppPathService {

        #region Declarations / Constructors

        protected readonly IEnvironmentService environment;
        protected readonly IFileSystemService fileSystem;

        public AppPathService() : this(new EnvironmentService(), new FileSystemService()) { }

        public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService) {
            this.environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Returns all valid video extensions.
        /// </summary>
        public string[] VideoExtensions => videoExtensions ?? (videoExtensions = new string[] { ".mp4", ".webm", ".avi", ".flv", ".mpg", ".mkv", ".wmv", ".tp", ".ts", ".mov", ".avs", ".m2v", ".vob" });
        private string[] videoExtensions;
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        public string[] AudioExtensions => audioExtensions ?? (audioExtensions = new string[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" });
        private string[] audioExtensions;
        /// <summary>
        /// Returns all valid image extensions.
        /// </summary>
        public string[] ImageExtensions => imageExtensions ?? (imageExtensions = new string[] { ".gif", ".jpg", ".png", ".bmp", ".tiff" });
        private string[] imageExtensions;

        /// <summary>
        /// Returns all extensions for specified media type.
        /// </summary>
        /// <param name="mediaType">The type of media for which to get extensions.</param>
        /// <returns>A string array of file extensions.</returns>
        public string[] GetMediaTypeExtensions(MediaType mediaType) {
            if (mediaType == MediaType.Video)
                return VideoExtensions;
            else if (mediaType == MediaType.Audio)
                return AudioExtensions;
            else if (mediaType == MediaType.Image)
                return ImageExtensions;
            else
                return new string[] { };
        }

        /// <summary>
        /// Returns the path where the settings file is stored.
        /// </summary>
        public string SettingsPath => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\Settings.xml");
        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        public string UnhandledExceptionLogPath => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\Log.txt");
        /// <summary>
        /// Returns the path where the database file is stored.
        /// </summary>
        public string DatabasePath => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\NaturalGroundingVideos.db");
        /// <summary>
        /// Returns the path where the blank initial database is stored.
        /// </summary>
        public string InitialDatabasePath => fileSystem.Path.Combine(environment.AppDirectory, "InitialDatabase.db");
        /// <summary>
        /// Returns the path where AviSynth plugins are located.
        /// </summary>
        public string AvisynthPluginsPath => fileSystem.Path.Combine(environment.AppDirectory, @"Encoder\");
        /// <summary>
        /// Returns the path where the 432hz Player is storing its Avisynth script during playback.
        /// </summary>
        public string Player432hzScriptFile => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzPlaying.avs");
        /// <summary>
        /// Returns the path where the 432hz Player settings file is stored.
        /// </summary>
        public string Player432hzConfigFile => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\432hzConfig.xml");
        /// <summary>
        /// Returns the path where the Powerlimnals Player settings file is stored.
        /// </summary>
        public string PowerliminalsPlayerConfigFile => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\PowerliminalsConfig.xml");

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
        public string SystemTempPath => fileSystem.Path.GetTempPath();
        /// <summary>
        /// Returns the temp path for the media downloader.
        /// </summary>
        public string DownloaderTempPath => fileSystem.Path.Combine(SystemTempPath, @"YangYoutubeDownloader\");
        //public string X264Path => @"Encoder\x264-10bit.exe";
        //public string Avs2yuvPath => @"Encoder\avs2yuv.exe";

        /// <summary>
        /// Returns the default Natural Grounding folder.
        /// </summary>
        public string DefaultNaturalGroundingFolder => environment.SystemRootDirectory + "Natural Grounding\\";

        /// <summary>
        /// Auto-detects SVP path.
        /// </summary>
        /// <returns>The default SVP path.</returns>
        public string GetDefaultSvpPath() {
            string[] DefaultPaths = new string[] {
                @"SVP 4\SVPManager.exe",
                @"SVP 4 Free\SVPManager.exe",
                @"SVP 4 Dev\SVPManager.exe",
                @"SVP\SVPMgr.exe"
            };
            string ProgramFiles = environment.ProgramFilesX86;
            string ItemPath;
            foreach (string item in DefaultPaths) {
                ItemPath = fileSystem.Path.Combine(ProgramFiles, item);
                if (fileSystem.File.Exists(ItemPath))
                    return ItemPath;
            }
            return fileSystem.Path.Combine(ProgramFiles, DefaultPaths[0]);
        }
    }
}
