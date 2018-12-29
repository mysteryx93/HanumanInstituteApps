using System.IO;
using DataAccess;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.Avisynth;

namespace Business {
    /// <summary>
    /// Handles 432hz auto-pitch features, which makes the music in better harmony with your heart.
    /// </summary>
    public class AutoPitchBusiness {
        /// <summary>
        /// Applies 432hz auto-pitch if file PixelAspectRatio is 1 and FPS can be read.
        /// </summary>
        /// <param name="video">The video for which to create the auto-pitch script file.</param>
        /// <returns>Whether auto-pitch is enabled for this video.</returns>
        public static bool AppyAutoPitch(Media video) {
            if (!File.Exists(Settings.NaturalGroundingFolder + video.FileName))
                return false;

            FFmpegProcess InfoReader = MediaInfo.GetFileInfo(Settings.NaturalGroundingFolder + video.FileName);
            if (Settings.SavedFile.ChangeAudioPitch && InfoReader?.VideoStream?.PixelAspectRatio == 1 && !video.DisablePitch && InfoReader?.VideoStream?.BitDepth == 8) {
                CreateScript(Settings.NaturalGroundingFolder + video.FileName, InfoReader);
                return true;
            } else
                return false;
        }

        public static string LastScriptPath { get; private set; } = null;

        public static void CreateScript(string inputFile, FFmpegProcess infoReader, string scriptPath = null) {
            //if (LastScriptPath != null)
            //    PathManager.SafeDelete(LastScriptPath);
            if (scriptPath == null)
                scriptPath = GetAutoPitchFilePath(inputFile, infoReader?.VideoStream?.Format);
            ChangePitchBusiness.CreateScript(inputFile, infoReader, scriptPath);
            LastScriptPath = scriptPath;
        }

        public static string GetAutoPitchFilePath(string fileName, string videoCodec) {
            return Path.Combine(PathManager.TempFilesPath, "Player.avs");
            //return string.Format("{0}432hz_{1}{2}.avs", PathManager.TempFilesPath, Path.GetFileNameWithoutExtension(fileName), string.IsNullOrEmpty(videoCodec) ? "" : ("_" + videoCodec));
            //return Regex.Replace(Result, @"[^\u0000-\u007F]+", string.Empty);
        }
    }
}
