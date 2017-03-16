using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using EmergenceGuardian.FFmpeg;

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

        public static void CreateScript(string inputFile, FFmpegProcess infoReader) {
            if (LastScriptPath != null)
                File.Delete(LastScriptPath);
            LastScriptPath = GetAutoPitchFilePath(inputFile, infoReader?.VideoStream?.Format);
            CreateScript(inputFile, infoReader, LastScriptPath);
        }

        public static string GetAutoPitchFilePath(string fileName, string videoCodec) {
            return string.Format("{0}{1}{2}.avs", Settings.TempFilesPath, Path.GetFileNameWithoutExtension(fileName) , string.IsNullOrEmpty(videoCodec) ? "" : ("_" + videoCodec));
        }

        /// <summary>
        /// Creates an AviSynth script that will auto-pitch the audio to 432hz. You then open this script file in the video player instead of directly opening the file.
        /// </summary>
        /// <param name="inputFile">The video to play.</param>
        /// <param name="infoReader">An object to read media information.</param>
        public static void CreateScript(string inputFile, FFmpegProcess infoReader, string scriptLocation) {
            bool AviSynthPlus = MpcConfigBusiness.GetAviSynthVersion() == AviSynthVersion.AviSynthPlus;
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            if (AviSynthPlus) {
                //Script.AppendLine(@"SetFilterMTMode(""DEFAULT_MT_MODE"",2)");
                //Script.AppendLine(@"SetFilterMTMode(""LWLibavVideoSource"",3)");
                //Script.AppendLine(@"SetFilterMTMode(""LWLibavAudioSource"",3)");
                Script.OpenDirect(inputFile, infoReader.AudioStream != null, infoReader.VideoStream != null);
                Script.AppendLine("Preroll(int(FrameRate*3))");
                // This causes a slight audio delay in AviSynth 2.6
                Script.LoadPluginDll("TimeStretch.dll");
                Script.AppendLine("ResampleAudio(48000)");
                Script.AppendLine("TimeStretchPlugin(pitch = 100.0 * 0.98181819915771484)");
                //Script.AppendLine("Prefetch({0})", CPU);
            } else {
                int CPU = Environment.ProcessorCount / 2;
                Script.AppendLine("SetMTMode(3,{0})", CPU);
                Script.OpenDirect(inputFile, infoReader.AudioStream != null, infoReader.VideoStream != null);
                Script.AppendLine("SetMTMode(2)");
                Script.AppendLine("Preroll(int(FrameRate*3))");
                //Script.AppendLine("Loop(int(FrameRate/2), 0, 0)");
                //Script.LoadPluginAvsi("UUSize4.avsi");
                //Script.AppendLine("UUSize4(mod=4)");
                // This slightly slows down playback speed but audio stays in sync
                Script.AppendLine("V = AssumeFPS(432.0 / 440.0 * FrameRate)");
                Script.AppendLine("A = AssumeSampleRate(int(432.0 / 440.0 * AudioRate))");
                Script.AppendLine("AudioDub(V, A)");
            }

            Script.Cleanup();
            Script.WriteToFile(scriptLocation);
        }
    }
}
