using HanumanInstitute.AvisynthScriptBuilder;
using HanumanInstitute.CommonServices;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.NaturalGroundingPlayer.DataAccess;
using HanumanInstitute.NaturalGroundingPlayer.Services;

namespace HanumanInstitute.NaturalGroundingPlayer.Business
{

    #region Interface

    /// <summary>
    /// Handles 432hz auto-pitch features, which makes the music in better harmony with your heart.
    /// </summary>
    public interface IAutoPitchBusiness
    {
        /// <summary>
        /// Applies 432hz auto-pitch if file PixelAspectRatio is 1 and FPS can be read.
        /// </summary>
        /// <param name="video">The video for which to create the auto-pitch script file.</param>
        /// <returns>Whether auto-pitch is enabled for this video.</returns>
        bool AppyAutoPitch(Media video);
        /// <summary>
        /// Returns the name of the last generated script file.
        /// </summary>
        string? LastScriptPath { get; }
        /// <summary>
        /// Generates a script to play specified file at 432hz.
        /// </summary>
        /// <param name="inputFile">The file to play.</param>
        /// <param name="fileInfo">An object containing media file information.</param>
        /// <param name="scriptPath">Specified where to create the file. If not specified, it will be created in the system temp folder.</param>
        void GenerateScript(string inputFile, FileInfoFFmpeg fileInfo, string? scriptPath = null);
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
    public class AutoPitchBusiness : IAutoPitchBusiness
    {
        private readonly IFileSystemService fileSystem;
        private readonly IAppPathService appPath;
        private readonly IAppSettingsProvider settings;
        private readonly IMediaInfoReader mediaInfo;
        private readonly IChangePitchBusiness changePitch;

        public AutoPitchBusiness(IFileSystemService fileSystemService, IAppPathService appPathService, IAppSettingsProvider settings, IMediaInfoReader mediaInfo, IChangePitchBusiness changePitchBusiness)
        {
            fileSystem = fileSystemService;
            appPath = appPathService;
            this.settings = settings;
            this.mediaInfo = mediaInfo;
            changePitch = changePitchBusiness;
        }

        /// <summary>
        /// Applies 432hz auto-pitch if file PixelAspectRatio is 1 and FPS can be read.
        /// </summary>
        /// <param name="video">The video for which to create the auto-pitch script file.</param>
        /// <returns>Whether auto-pitch is enabled for this video.</returns>
        public bool AppyAutoPitch(Media video)
        {
            if (!fileSystem.File.Exists(settings.Value.NaturalGroundingFolder + video.FileName))
            {
                return false;
            }

            var fileInfo = mediaInfo.GetFileInfo(settings.Value.NaturalGroundingFolder + video.FileName);
            if (settings.Value.ChangeAudioPitch && fileInfo?.VideoStream?.PixelAspectRatio == 1 && !video.DisablePitch && fileInfo?.VideoStream?.BitDepth == 8)
            {
                GenerateScript(settings.Value.NaturalGroundingFolder + video.FileName, fileInfo);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the name of the last generated script file.
        /// </summary>
        public string? LastScriptPath { get; private set; }

        /// <summary>
        /// Generates a script to play specified file at 432hz.
        /// </summary>
        /// <param name="inputFile">The file to play.</param>
        /// <param name="fileInfo">An object containing media file information.</param>
        /// <param name="scriptPath">Specified where to create the file. If not specified, it will be created in the system temp folder.</param>
        public void GenerateScript(string inputFile, FileInfoFFmpeg fileInfo, string? scriptPath = null)
        {
            fileInfo.CheckNotNull(nameof(fileInfo));

            //if (LastScriptPath != null)
            //    PathManager.SafeDelete(LastScriptPath);
            if (scriptPath == null)
            {
                scriptPath = GetAutoPitchFilePath(inputFile, fileInfo.VideoStream?.Format);
            }

            changePitch.GenerateScript(inputFile, fileInfo, scriptPath);
            LastScriptPath = scriptPath;
        }

        /// <summary>
        /// Returns a default file path where to create the script.
        /// </summary>
        /// <param name="fileName">The path of the file to play.</param>
        /// <param name="videoCodec">The media file video codec.</param>
        /// <returns><A file path./returns>
        public string GetAutoPitchFilePath(string fileName, string? videoCodec)
        {
            return fileSystem.Path.Combine(appPath.SystemTempPath, "Player.avs");
            //return string.Format("{0}432hz_{1}{2}.avs", PathManager.TempFilesPath, Path.GetFileNameWithoutExtension(fileName), string.IsNullOrEmpty(videoCodec) ? "" : ("_" + videoCodec));
            //return Regex.Replace(Result, @"[^\u0000-\u007F]+", string.Empty);
        }
    }
}
