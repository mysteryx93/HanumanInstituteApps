using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.Avisynth {
    public static class AvisynthTools {
        /// <summary>
        /// Gets an AviSynth clip information by running a script that outputs the frame count to a file.
        /// </summary>
        /// <param name="source">The AviSynth script to get information about.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>The frame count.</returns>
        public static long GetFrameCount(string source, ProcessStartOptions options) {
            if (!File.Exists(source))
                return 0;
            string TempScriptBase = Path.ChangeExtension(source, null);
            string TempScript = PathManager.GetTempFile("avs");
            string TempResult = Path.ChangeExtension(TempScript, "txt");

            AviSynthScriptBuilder Script;
            if (source.ToLower().EndsWith(".avs")) {
                // Read source script and remove MT. Also remove Deshaker if present.
                string FileContent = File.ReadAllText(source);
                FileContent.Replace(Environment.NewLine + "Deshaker", Environment.NewLine + "#Deshaker");
                Script = new AviSynthScriptBuilder(FileContent);
                Script.RemoveMT();
            } else {
                // Generate script to read media file.
                Script = new AviSynthScriptBuilder();
                Script.AddPluginPath();
                Script.OpenDirect(source, false);
            }
            // Get frame count.
            Script.AppendLine();
            Script.AppendLine(@"WriteFileStart(""{0}"", ""FrameRate""{1}""Framecount"")", TempResult, @", """""" "","" """""", ");
            Script.AppendLine("Trim(0,-1)");
            Script.WriteToFile(TempScript);

            // Run script.
            FFmpegProcess Worker = new FFmpegProcess(options);
            Worker.RunAvisynth(TempScript);

            // Read frame count
            long Result = 0;
            if (File.Exists(TempResult)) {
                string FileString = File.ReadAllText(TempResult);
                string[] FileValues = FileString.Split(',');
                try {
                    //Result.FrameRate = float.Parse(FileValues[0], CultureInfo.InvariantCulture);
                    Result = int.Parse(FileValues[1]);
                }
                catch {
                }
            }

            // Delete temp files.
            File.Delete(TempScript);
            File.Delete(TempResult);

            return Result;
        }

        /// <summary>
        /// Returns the audio gain that can be applied to an audio file.
        /// </summary>
        /// <param name="settings">The source file to analyze.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>A float value representing the audio gain that can be applied, or null if it failed.</returns>
        public static float? GetAudioGain(string filePath, ProcessStartOptions options) {
            FFmpegProcess Worker = new FFmpegProcess(options);
            string Args = string.Format(@"-i ""{0}"" -af ""volumedetect"" -f null NUL", filePath);
            Worker.RunFFmpeg(Args);
            float? Result = null;
            string FileString = Worker.Output;
            // Find max_volume.
            string SearchVal = "max_volume: ";
            int Pos1 = FileString.IndexOf(SearchVal);
            if (Pos1 >= 0) {
                Pos1 += SearchVal.Length;
                // Find end of line.
                int Pos2 = FileString.IndexOf('\r', Pos1);
                if (Pos2 >= 0) {
                    string MaxVolString = FileString.Substring(Pos1, Pos2 - Pos1);
                    if (MaxVolString.Length > 3) {
                        // Remove ' dB'
                        MaxVolString = MaxVolString.Substring(0, MaxVolString.Length - 3);
                        float MaxVol = 0;
                        if (float.TryParse(MaxVolString, out MaxVol))
                            Result = Math.Abs(MaxVol);
                    }
                }
            }
            return Result;
        }
    }
}
