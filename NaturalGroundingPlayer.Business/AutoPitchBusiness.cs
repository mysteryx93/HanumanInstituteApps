using System;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.Encoder;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;
using EmergenceGuardian.AvisynthScriptBuilder;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Handles 432hz auto-pitch features, which makes the music in better harmony with your heart.
    /// </summary>
    public interface IAutoPitchBusiness {
        /// <summary>
        /// Applies 432hz auto-pitch if file PixelAspectRatio is 1 and FPS can be read.
        /// </summary>
        /// <param name="video">The video for which to create the auto-pitch script file.</param>
        /// <returns>Whether auto-pitch is enabled for this video.</returns>
        bool AppyAutoPitch(Media video);
        /// <summary>
        /// Returns the name of the last generated script file.
        /// </summary>
        string LastScriptPath { get;  }
        /// <summary>
        /// Generates a script to play specified file at 432hz.
        /// </summary>
        /// <param name="inputFile">The file to play.</param>
        /// <param name="fileInfo">An object containing media file information.</param>
        /// <param name="scriptPath">Specified where to create the file. If not specified, it will be created in the system temp folder.</param>
        void GenerateScript(string inputFile, IFileInfoFFmpeg fileInfo, string scriptPath = null);
        /// <summary>
        /// Returns a default file path where to create the script.
        /// </summary>
        /// <param name="fileName">The path of the file to play.</param>
        /// <param name="videoCodec">The media file video codec.</param>
        /// <returns><A file path./returns>
        string GetAutoPitchFilePath(string fileName, string videoCodec);
    }

    #endregion

    /// <summary>
    /// Handles 432hz auto-pitch features, which makes the music in better harmony with your heart.
    /// </summary>
    public class AutoPitchBusiness : IAutoPitchBusiness {

        #region Declarations / Constructors

        protected readonly IFileSystemService fileSystem;
        protected readonly IAppPathService appPath;
        protected readonly ISettings settings;
        private IMediaInfoReader mediaInfo;
        private IChangePitchBusiness changePitch;

        public AutoPitchBusiness(IFileSystemService fileSystemService, IAppPathService appPathService, ISettings settings, IMediaInfoReader mediaInfo, IChangePitchBusiness changePitchBusiness) {
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.appPath = appPathService ?? throw new ArgumentNullException(nameof(appPathService));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
            this.changePitch = changePitchBusiness ?? throw new ArgumentNullException(nameof(changePitchBusiness));
        }

        #endregion

        /// <summary>
        /// Applies 432hz auto-pitch if file PixelAspectRatio is 1 and FPS can be read.
        /// </summary>
        /// <param name="video">The video for which to create the auto-pitch script file.</param>
        /// <returns>Whether auto-pitch is enabled for this video.</returns>
        public bool AppyAutoPitch(Media video) {
            if (!fileSystem.File.Exists(settings.NaturalGroundingFolder + video.FileName))
                return false;

            IFileInfoFFmpeg fileInfo = mediaInfo.GetFileInfo(settings.NaturalGroundingFolder + video.FileName);
            if (settings.Data.ChangeAudioPitch && fileInfo?.VideoStream?.PixelAspectRatio == 1 && !video.DisablePitch && fileInfo?.VideoStream?.BitDepth == 8) {
                GenerateScript(settings.NaturalGroundingFolder + video.FileName, fileInfo);
                return true;
            } else
                return false;
        }

        /// <summary>
        /// Returns the name of the last generated script file.
        /// </summary>
        public string LastScriptPath { get; private set; } = null;

        /// <summary>
        /// Generates a script to play specified file at 432hz.
        /// </summary>
        /// <param name="inputFile">The file to play.</param>
        /// <param name="fileInfo">An object containing media file information.</param>
        /// <param name="scriptPath">Specified where to create the file. If not specified, it will be created in the system temp folder.</param>
        public void GenerateScript(string inputFile, IFileInfoFFmpeg fileInfo, string scriptPath = null) {
            //if (LastScriptPath != null)
            //    PathManager.SafeDelete(LastScriptPath);
            if (scriptPath == null)
                scriptPath = GetAutoPitchFilePath(inputFile, fileInfo?.VideoStream?.Format);
            changePitch.GenerateScript(inputFile, fileInfo, scriptPath);
            LastScriptPath = scriptPath;
        }

        /// <summary>
        /// Returns a default file path where to create the script.
        /// </summary>
        /// <param name="fileName">The path of the file to play.</param>
        /// <param name="videoCodec">The media file video codec.</param>
        /// <returns><A file path./returns>
        public string GetAutoPitchFilePath(string fileName, string videoCodec) {
            return fileSystem.Path.Combine(appPath.SystemTempPath, "Player.avs");
            //return string.Format("{0}432hz_{1}{2}.avs", PathManager.TempFilesPath, Path.GetFileNameWithoutExtension(fileName), string.IsNullOrEmpty(videoCodec) ? "" : ("_" + videoCodec));
            //return Regex.Replace(Result, @"[^\u0000-\u007F]+", string.Empty);
        }
    }
}
