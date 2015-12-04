using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    public class FfmpegBusiness {
        /// <summary>
        /// Merge audio and video files.
        /// </summary>
        /// <param name="videoFile">The video file with missing audio.</param>
        /// <param name="audioFile">The video file containing the audio.</param>
        /// <param name="destination">The output file.</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool JoinAudioVideo(string videoFile, string audioFile, string destination, bool silent) {
            File.Delete(destination);
            if (!string.IsNullOrEmpty(audioFile))
                return RunFfmpeg(string.Format(@"-i ""{0}"" -i ""{1}"" -acodec copy -vcodec copy -map 0:v -map 1:a ""{2}""", videoFile, audioFile, destination), silent);
            else
                return RunFfmpeg(string.Format(@"-i ""{0}"" -vcodec copy ""{1}""", videoFile, destination), silent);
        }

        /// <summary>
        /// Converts specified file into AVI format.
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="destination">The destination file, ending with .AVI</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool ConvertToAVI(string source, string destination, bool silent) {
            File.Delete(destination);
            // -vcodec huffyuv or utvideo, -acodec pcm_s16le
            bool Success = RunFfmpeg(string.Format(@"-i ""{0}"" -vcodec utvideo -acodec pcm_s16le ""{1}""", source, destination), silent);
            if (!Success)
                File.Delete(destination);
            return Success;
        }

        /// <summary>
        /// Extracts the video stream from specified video file.
        /// </summary>
        /// <param name="source">The video file to extract from.</param>
        /// <param name="destination">The output file to create.</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool ExtractVideo(string source, string destination, bool silent) {
            MediaInfoReader MediaReader = new MediaInfoReader();
            File.Delete(destination);
            return RunFfmpeg(string.Format(@"-i ""{0}"" -vcodec copy -an ""{1}""", source, destination), silent);
        }

        /// <summary>
        /// Extracts the audio from specified video.
        /// </summary>
        /// <param name="source">The video to extract the audio from.</param>
        /// <param name="destination">The destination file</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool ExtractAudio(string source, string destination) {
            File.Delete(destination);
            return RunFfmpeg(string.Format(@"-i ""{0}"" -vn -acodec copy ""{1}""", source, destination), false);
        }

        private static bool Run(string command, string arguments, bool silent) {
            return Run(command, arguments, silent, ProcessPriorityClass.Normal);
        }

        private static bool Run(string command, string arguments, bool silent, ProcessPriorityClass priority) {
            Process P = new Process();
            if (silent) {
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //P.StartInfo.RedirectStandardError = true;
                //P.StartInfo.UseShellExecute = false;
            }
            P.StartInfo.FileName = command;
            P.StartInfo.Arguments = arguments;
            // P.ErrorDataReceived += P_ErrorDataReceived;
            P.Start();
            P.WaitForExit();
            // ExitCode is 0 for normal exit. Different value when closing the console.
            return P.ExitCode == 0;
        }

        /// <summary>
        /// Runs FFMPEG with specified arguments.
        /// </summary>
        /// <param name="arguments">FFMPEG startup arguments.</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        /// <returns>Whether the operation was completed.</returns>
        private static bool RunFfmpeg(string arguments, bool silent) {
            return Run(@"Encoder\ffmpeg.exe", arguments, silent);
        }

        /// <summary>
        /// Converts specified file into H264 format. The script file must already be written.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool ConvertToH264(MediaEncoderSettings settings) {
            File.Delete(settings.OutputFile);
            string PipeArgs = string.Format(@"/c Encoder\avs2yuv.exe ""{0}"" -o - | Encoder\ffmpeg.exe -y -i - -an -c:v libx264 -preset {1} -crf {2} -psy-rd 1:0.05 ""{3}""", 
                settings.ScriptFile, settings.EncodePreset, settings.EncodeQuality, settings.OutputFile);
            return Run("cmd", PipeArgs, false);
        }
        
        /// <summary>
        /// Saves the audio output of specified script into a WAV file.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        public static void SaveAudioToWav(MediaEncoderSettings settings) {
            string TempFile = settings.TempFile + ".avs";
            // Read source script.
            string FileContent = File.ReadAllText(settings.ScriptFile);
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(FileContent);
            // Remote MT code.
            Script.ConvertForPreview();
            // Add audio gain.
            if (settings.AudioGain.HasValue && settings.AudioGain != 0) {
                Script.AppendLine("AmplifydB({0})", settings.AudioGain.Value);
            }
            if (settings.ChangeAudioPitch) {
                // Change pitch to 432hz.
                Script.LoadPluginDll("TimeStretch.dll");
                Script.AppendLine("ResampleAudio(48000)");
                Script.AppendLine("TimeStretchPlugin(pitch = 100.0 * 0.98181819915771484)");
            }
            Script.AppendLine("ConvertAudioTo16bit()");
            // Add TWriteWAV.
            Script.AppendLine();
            Script.LoadPluginDll("TWriteAVI.dll");
            Script.AppendLine(@"TWriteWAV(""{0}"", true)", Script.GetAsciiPath(settings.AudioFileWav));
            Script.AppendLine("ForceProcessWAV()");
            // Write temp script.
            Script.WriteToFile(TempFile);
            // Execute.
            // It aways returns an error but file is generated.
            string Args = string.Format(@"""{0}"" -o -", TempFile);
            Run("Encoder\\avs2yuv.exe", Args, false);
            File.Delete(TempFile);
        }

        public static bool EncodeAudio(MediaEncoderSettings settings) {
            string Args = string.Format(@"-q {0} -if ""{1}"" -of ""{2}""", settings.AudioQuality / 100f, settings.AudioFileWav, settings.AudioFileAac);
            return Run("Encoder\\NeroAacEnc.exe", Args, false);
        }

        /// <summary>
        /// Returns the audio gain that can be applied to an audio file.
        /// </summary>
        /// <param name="settings">The settings pointing to the file to analyze.</param>
        /// <returns>A float value representing the audio gain that can be applied, or null if it failed.</returns>
        public static float? GetAudioGain(MediaEncoderSettings settings) {
            string TempResult = settings.TempFile + ".txt";
            string Args = string.Format(@"/c Encoder\\ffmpeg.exe -i ""{0}"" -af ""volumedetect"" -f null null > ""{1}"" 2>&1", 
                Settings.NaturalGroundingFolder + settings.FileName, TempResult);
            Run("cmd", Args, true);
            float? Result = null;
            if (File.Exists(TempResult)) {
                string FileString = File.ReadAllText(TempResult);
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
            }
            return Result;
        }

        public static Rect GetAutoCropRect(MediaEncoderSettings settings, bool silent) {
            string TempScript = settings.TempFile + ".avs";
            string TempResult = settings.TempFile + ".txt";
            string TempOut = settings.TempFile + ".y4m";

            // Create script to get auto-crop coordinates
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            Script.LoadPluginDll("LSMASHSource.dll");
            Script.AppendLine(@"LWLibavVideoSource(""{0}"", cache=false, threads=1)", Script.GetAsciiPath(Settings.NaturalGroundingFolder + settings.FileName));
            Script.LoadPluginDll("RoboCrop26.dll");
            Script.AppendLine(@"RoboCrop(LogFn=""{0}"")", Script.GetAsciiPath(TempResult));
            Script.AppendLine("Trim(0,-1)");
            Script.WriteToFile(TempScript);

            // Run script.
            Run(@"Encoder\avs2yuv.exe", String.Format(@"""{0}"" -o {1}", TempScript, TempOut), true);

            // Read auto crop coordinates
            Rect Result = new Rect();
            if (File.Exists(TempResult)) {
                string[] Values = File.ReadAllText(TempResult).Split(' ');
                if (Values.Length >= 13) {
                    Result.Left = int.Parse(Values[10]);
                    Result.Top = int.Parse(Values[11]);
                    Result.Right = settings.SourceWidth.Value - int.Parse(Values[12]);
                    Result.Bottom = settings.SourceHeight.Value - int.Parse(Values[13]);
                    // Make result conservative, we have to visually see a line of black border to know the right dimensions.
                    if (Result.Left > 0)
                        Result.Left--;
                    if (Result.Top > 0)
                        Result.Top--;
                    if (Result.Right > 0)
                        Result.Right--;
                    if (Result.Bottom > 0)
                        Result.Bottom--;
                }
            }

            // Delete temp files.
            File.Delete(TempScript);
            File.Delete(TempResult);
            File.Delete(TempOut); // Dummy file that received avs2yuv output.

            return Result;
        }

        /// <summary>
        /// Gets the total count of frames in an AviSynth script by running a script that outputs the frame count to a file.
        /// </summary>
        /// <param name="source">The AviSynth script to get frame count for.</param>
        /// <param name="silent">If true, the x264 window will be hidden.</param>
        /// <returns>The video frame count.</returns>
        public static int GetFrameCount(MediaEncoderSettings settings, bool silent) {
            string TempScript = settings.TempFile + ".avs";
            string TempResult = settings.TempFile + ".txt";
            string TempOut = settings.TempFile + ".y4m";

            // Create script to get frame count.
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AppendLine(@"AviSource(""{0}"")", settings.ScriptFile);
            Script.AppendLine(@"WriteFileStart(""{0}"", ""Framecount()"")", TempResult);
            Script.AppendLine("Trim(0,-1)");
            Script.ConvertForPreview();
            Script.WriteToFile(TempScript);

            // Run script.
            Run(@"Encoder\avs2yuv.exe", String.Format(@"""{0}"" -o {1}", TempScript, TempOut), true);

            // Read frame count
            int FrameCount = 0;
            if (File.Exists(TempResult)) {
                string FrameCountString = File.ReadAllText(TempResult);
                Int32.TryParse(FrameCountString, out FrameCount);
            }

            // Delete temp files.
            File.Delete(TempScript);
            File.Delete(TempResult);
            File.Delete(TempOut); // Dummy file that received avs2yuv output.

            return FrameCount;
        }
    }
}
