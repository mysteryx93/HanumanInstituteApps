using System;
using System.IO;
using System.Linq;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.Avisynth {
    /// <summary>
    /// Handles 432hz auto-pitch features, which makes the music in better harmony with your heart.
    /// </summary>
    public class ChangePitchBusiness {
        /// <summary>
        /// Creates an AviSynth script that will auto-pitch the audio to 432hz. You then open this script file in the video player instead of directly opening the file.
        /// </summary>
        /// <param name="inputFile">The video to play.</param>
        /// <param name="infoReader">An object to read media information.</param>
        public static void CreateScript(string inputFile, FFmpegProcess infoReader, string scriptLocation) {
            bool AviSynthPlus = AvisynthEnv.GetAvisynthVersion() == AvisynthVersion.AviSynthPlus;
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            if (AviSynthPlus) {
                //Script.AppendLine(@"SetFilterMTMode(""DEFAULT_MT_MODE"",2)");
                //Script.AppendLine(@"SetFilterMTMode(""LWLibavVideoSource"",3)");
                //Script.AppendLine(@"SetFilterMTMode(""LWLibavAudioSource"",3)");
                bool IsAudio = PathManager.AudioExtensions.Contains(Path.GetExtension(inputFile).ToLower());
                Script.OpenDirect(inputFile, infoReader.AudioStream != null, !IsAudio && infoReader.VideoStream != null);
                if (IsAudio)
                    Script.AppendLine("AudioDub(BlankClip(Last, width=8, height=8), Last)");
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
