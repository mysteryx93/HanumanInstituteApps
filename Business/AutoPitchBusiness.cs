using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

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
            using (MediaInfoReader InfoReader = new MediaInfoReader()) {
                InfoReader.LoadInfo(Settings.NaturalGroundingFolder + video.FileName);
                if (Settings.SavedFile.ChangeAudioPitch && InfoReader.PixelAspectRatio == 1 && !video.DisablePitch) {
                    CreateScript(Settings.NaturalGroundingFolder + video.FileName, InfoReader);
                    return true;
                } else
                    return false;
            }
        }

        /// <summary>
        /// Creates an AviSynth script that will auto-pitch the audio to 432hz. You then open this script file in the video player instead of directly opening the file.
        /// </summary>
        /// <param name="inputFile">The video to play.</param>
        /// <param name="infoReader">An object to read media information.</param>
        public static void CreateScript(string inputFile, MediaInfoReader infoReader) {
            int CPU = Environment.ProcessorCount;
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            Script.LoadPluginDll("LSMASHSource.dll");
            Script.LoadPluginDll("TimeStretch.dll");
            Script.LoadPluginAvsi("UUSize4.avsi");
            Script.AppendLine("SetMTMode(3,{0})", CPU);
            Script.AppendLine(@"file = ""{0}""", Script.GetAsciiPath(inputFile));

            //if (new string[] { ".mp4", ".mov" }.Contains(Path.GetExtension(inputFile).ToLower())) {
            //    Script.AppendLine("LSMASHVideoSource(file, threads=1)");
            //    Script.AppendLine("AudioDub(LSMASHAudioSource(file))");
            //} else {
            //}
            Script.AppendLine("LWLibavVideoSource(file, cache=false, threads=1)");
            Script.AppendLine("AudioDub(LWLibavAudioSource(file, cache=false))");

            Script.AppendLine("SetMTMode(2)");
            Script.AppendLine("UUSize4(mod=4)");
            Script.AppendLine("ResampleAudio(48000)");
            Script.AppendLine("TimeStretchPlugin(pitch = 100.0 * 0.98181819915771484)");
            Script.WriteToFile(Settings.AutoPitchFile);
        }
    }
}
