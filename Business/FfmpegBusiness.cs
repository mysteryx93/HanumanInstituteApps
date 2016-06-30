using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        /// <param name="fixAac">FFMPEG-encoded AAC streams are invalid and require an extra flag to join.</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool JoinAudioVideo(string videoFile, string audioFile, string destination, bool fixAac, bool silent) {
            bool Result = true;
            File.Delete(destination);

            // FFMPEG fails to muxe H264 into MKV container. Converting to MP4 and then muxing with the audio, however, works.
            string OriginalVideoFile = videoFile;
            if ((videoFile.EndsWith(".264") || videoFile.EndsWith(".265")) && destination.EndsWith(".mkv")) {
                videoFile = videoFile.Substring(0, videoFile.Length - 4) + ".mp4";
                Result = JoinAudioVideo(OriginalVideoFile, null, videoFile, false, silent);
            }

            if (Result) {
                // Join audio and video files.
                if (!string.IsNullOrEmpty(audioFile)) {
                    Result = RunFfmpeg(string.Format(@"-i ""{0}"" -i ""{1}"" -acodec copy -vcodec copy -map 0:v -map 1:a{2} ""{3}""", videoFile, audioFile, fixAac ? " -bsf:a aac_adtstoasc" : "", destination), silent);
                } else
                    Result = RunFfmpeg(string.Format(@"-i ""{0}"" -vcodec copy ""{1}""", videoFile, destination), silent);
            }

            // Delete temp file.
            if (OriginalVideoFile != videoFile)
                File.Delete(videoFile);
            return Result;
        }

        //public static bool JoinAudioVideo(string videoFile, string audioFile, string destination, bool silent) {
        //    File.Delete(destination);
        //    if (!string.IsNullOrEmpty(audioFile))
        //        return RunFfmpeg(string.Format(@"-i ""{0}"" -i ""{1}"" -acodec copy -vcodec copy -map 0:v -map 1:a ""{2}""", videoFile, audioFile, destination), silent);
        //    else
        //        return RunFfmpeg(string.Format(@"-i ""{0}"" -vcodec copy ""{1}""", videoFile, destination), silent);
        //}



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
            try {
                if (!P.HasExited)
                    P.PriorityClass = priority;
            }
            catch { }
            P.WaitForExit();
            // ExitCode is 0 for normal exit. Different value when closing the console.
            return P.ExitCode == 0;
        }

        private static string RunToString(string command, string arguments) {
            Process P = new Process();
            P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            P.StartInfo.CreateNoWindow = true;
            P.StartInfo.RedirectStandardError = true;
            P.StartInfo.UseShellExecute = false;
            P.StartInfo.FileName = command;
            P.StartInfo.Arguments = arguments;
            P.Start();
            P.WaitForExit();
            return P.StandardError.ReadToEnd();
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
        /// Encodes specified file into H264 format. The script file must already be written.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool EncodeH264(MediaEncoderSettings settings) {
            File.Delete(settings.OutputFile);
            string PipeArgs;
            //if (settings.Encode10bit) {
            //    // x264-10b provides 10-bit encoding.
            //    settings.CalculateSize();
            //    ClipInfo Clip = GetClipInfo(settings, true);
            //    if (Clip == null) // Sometimes it fails; try again
            //        Clip = GetClipInfo(settings, true);
            //    //PipeArgs = string.Format(@"/c Encoder\avs4x26x --x26x-binary ""Encoder\x264-10bit.exe"" --depth 16 ""{0}"" --preset {1} --crf {2} --psy-rd 1:0.05 --output ""{3}"" --frames {4} -",
            //    //    settings.ScriptFile, settings.EncodePreset, settings.EncodeQuality, settings.OutputFile, Clip.FrameCount);
            //    if (settings.VideoCodec == VideoCodecs.x264) {
            //        PipeArgs = string.Format(@"/c Encoder\avs2yuv.exe -raw ""{0}"" -o - | Encoder\x264-10bit.exe --demuxer raw --input-depth 16 --input-res {1}x{2} --fps {3} --preset {4} --crf {5} --psy-rd 1:0.05 --output ""{6}"" --frames {7} -",
            //            settings.ScriptFile, settings.OutputWidth, settings.OutputHeight, Clip.FrameRate,
            //            settings.EncodePreset, settings.EncodeQuality, settings.OutputFile, Clip.FrameCount);
            //    } else {
            //        PipeArgs = string.Format(@"/c Encoder\avs2yuv.exe -raw ""{0}"" -o - | Encoder\x265-10bit.exe --input-depth 16 --input-res {1}x{2} --fps {3} --preset {4} --crf {5} --output ""{6}"" --frames {7} -",
            //            settings.ScriptFile, settings.OutputWidth, settings.OutputHeight, Clip.FrameRate,
            //            settings.EncodePreset, settings.EncodeQuality, settings.OutputFile, Clip.FrameCount);
            //    }
            //} else {
            // Ffmpeg contains x264 with 8-bit encoding.
            if (settings.VideoCodec == VideoCodecs.x264) {
                PipeArgs = string.Format(@"/c {0}avs2yuv.exe ""{1}"" -o - | {0}ffmpeg.exe -y -i - -an -c:v libx264 -psy-rd 1:0.05 -preset {2} -crf {3} ""{4}""",
                    Settings.AviSynthPluginsPath, settings.ScriptFile, settings.EncodePreset, settings.EncodeQuality, settings.OutputFile);
            } else {
                PipeArgs = string.Format(@"/c {0}avs2yuv.exe ""{1}"" -o - | {0}ffmpeg.exe -y -i - -an -c:v libx265 -preset {2} -crf {3} ""{4}""",
                    Settings.AviSynthPluginsPath, settings.ScriptFile, settings.EncodePreset, settings.EncodeQuality, settings.OutputFile);
            }
            //}
            return Run("cmd", PipeArgs, false);
        }

        public static bool RunDeshakerPass(MediaEncoderSettings settings) {
            string TempOut = settings.TempFile + ".avi";
            string Args = string.Format(@"""{0}"" -o -", settings.DeshakerScript);

            string PipeArgs = string.Format(@"/c {0}avs2yuv.exe ""{1}"" -o - | {0}ffmpeg.exe -y -i - -an ""{2}""",
                Settings.AviSynthPluginsPath, settings.DeshakerScript, TempOut);
            bool Result = Run("cmd", PipeArgs, false);
            File.Delete(TempOut);
            return Result;
        }

        /// <summary>
        /// Saves the audio output of specified script into a WAV file.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        public static void SaveAudioToWav(MediaEncoderSettings settings, bool silent) {
            string TempFile = settings.TempFile + ".avs";
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            if (settings.VideoCodec != VideoCodecs.Copy) {
                // Read source script.
                Script.Script = File.ReadAllText(settings.ScriptFile);
                // Remote MT code.
                Script.RemoveMT();
                Script.AppendLine("Trim(0,0)");
            } else {
                // Read full video file.
                Script.AddPluginPath();
                if (settings.ConvertToAvi || settings.InputFile.ToLower().EndsWith(".avi"))
                    Script.OpenAvi(settings.InputFile, !string.IsNullOrEmpty(settings.SourceAudioFormat));
                else
                    Script.OpenDirect(settings.InputFile, !string.IsNullOrEmpty(settings.SourceAudioFormat));
                Script.AppendLine("KillVideo()");
            }
            Script.AppendLine();
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
            // Add TWriteWAV.
            Script.AppendLine();
            Script.LoadPluginDll("TWriteAVI.dll");
            Script.AppendLine(@"TWriteWAV(""{0}"", true)", Script.GetAsciiPath(settings.AudioFileWav));
            Script.AppendLine("ForceProcessWAV()");

            // Write temp script.
            Script.WriteToFile(TempFile);
            // Execute. It aways returns an error but file is generated.
            string Args = string.Format(@"""{0}"" -o -", TempFile);
            Run("Encoder\\avs2yuv.exe", Args, silent);
            File.Delete(TempFile);
        }

        public static bool EncodeAudio(MediaEncoderSettings settings, bool silent) {
            // string Args = string.Format(@"-q {0} -if ""{1}"" -of ""{2}""", settings.AudioQuality / 100f, settings.AudioFileWav, settings.AudioFileAac);
            // return Run("Encoder\\NeroAacEnc.exe", Args, true);
            string Args = string.Format(@"-i {2}{0} -b:a {1}k {3}",
                settings.AudioAction == AudioActions.EncodeOpus ? " -c:a libopus" : "",
                settings.AudioQuality,
                settings.AudioFileWav,
                settings.AudioAction == AudioActions.EncodeOpus ? settings.AudioFileOpus : settings.AudioFileAac);
            return Run("Encoder\\ffmpeg.exe", Args, true);
        }

        public static float? GetPixelAspectRatio(MediaEncoderSettings settings) {
            string ConsoleOut = RunToString("Encoder\\ffmpeg.exe", string.Format(@"-i ""{0}""", Settings.NaturalGroundingFolder + settings.FileName));
            int PosStart = ConsoleOut.IndexOf("[SAR ") + 5;
            int PosEnd = ConsoleOut.IndexOf(" DAR ", PosStart);
            if (PosStart < 0 || PosEnd < 0)
                return null;
            string SARText = ConsoleOut.Substring(PosStart, PosEnd - PosStart);
            string[] SAR = SARText.Split(':');
            if (SAR.Length != 2)
                return null;
            try {
                float Result = (float)Math.Round((decimal)int.Parse(SAR[0]) / int.Parse(SAR[1]), 3);
                return Result;
            }
            catch {
                return null;
            }
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
                File.Delete(TempResult);
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
            Script.OpenDirect(Settings.NaturalGroundingFolder + settings.FileName, false);
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
            Exception LastError = null;
            for (int i = 0; i < 10; i++) {
                try {
                    File.Delete(TempScript);
                    File.Delete(TempResult);
                    File.Delete(TempOut); // Dummy file that received avs2yuv output.
                    break;
                }
                catch (Exception e) {
                    LastError = e;
                    System.Threading.Thread.Sleep(500);
                }
            }
            if (LastError != null)
                throw LastError;

            return Result;
        }

        /// <summary>
        /// Gets an AviSynth clip information by running a script that outputs the frame count to a file.
        /// </summary>
        /// <param name="source">The AviSynth script to get information for.</param>
        /// <param name="silent">If true, the x264 window will be hidden.</param>
        /// <returns>The clip information.</returns>
        public static ClipInfo GetClipInfo(MediaEncoderSettings settings, bool silent) {
            string TempScript = settings.TempFile + ".avs";
            string TempResult = settings.TempFile + ".txt";
            string TempOut = settings.TempFile + ".y4m";

            // Read source script and remove MT.
            string FileContent = File.ReadAllText(settings.ScriptFile);
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(FileContent);
            Script.RemoveMT();
            //Script.DitherOut(false);

            // Get frame count.
            Script.AppendLine();
            Script.AppendLine(@"WriteFileStart(""{0}"", ""FrameRate""{1}""Framecount"")", TempResult, @", """""" "","" """""", ");
            Script.AppendLine("Trim(0,-1)");
            Script.WriteToFile(TempScript);

            // Run script.
            Run(@"Encoder\avs2yuv.exe", String.Format(@"""{0}"" -o {1}", TempScript, TempOut), true);

            // Read frame count
            ClipInfo Result = null;
            if (File.Exists(TempResult)) {
                string FileString = File.ReadAllText(TempResult);
                string[] FileValues = FileString.Split(',');
                Result = new ClipInfo();
                try {
                    Result.FrameRate = float.Parse(FileValues[0], CultureInfo.InvariantCulture);
                    Result.FrameCount = int.Parse(FileValues[1]);
                }
                catch {
                    Result = null;
                }
            }

            // Delete temp files.
            File.Delete(TempScript);
            File.Delete(TempResult);
            File.Delete(TempOut); // Dummy file that received avs2yuv output.

            return Result;
        }

        /// <summary>
        /// Returns which version of OpenCL the GPU supports.
        /// </summary>
        /// <returns>The version of OpenCL supported by the GPU.</returns>
        public static SupportedOpenClVersion TestSupportedOpenClVersion() {
            if (GpuSupportsOpenCL(true))
                return SupportedOpenClVersion.v12;
            else if (GpuSupportsOpenCL(false))
                return SupportedOpenClVersion.v11;
            else
                return SupportedOpenClVersion.None;
        }

        /// <summary>
        /// Returns whether the GPU supports the latest version of KNLMeans with OpenCL 1.2
        /// </summary>
        /// <param name="supports11">If true, it will instead test whether the GPU supports OpenCL 1.1</param>
        /// <returns>True if OpenCL is supported.</returns>
        private static bool GpuSupportsOpenCL(bool version12) {
            string TempScript = Settings.TempFilesPath + "Temp.avs";
            string TempResult = Settings.TempFilesPath + "Temp.txt";
            string TempOut = Settings.TempFilesPath + "Temp.y4m";

            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            Script.LoadPluginDll(string.Format("KNLMeansCL{0}.dll", version12 ? "" : "-6.11"));
            Script.AppendLine(@"colorbars(pixel_type = ""yv12"").killaudio().trim(1, 1)");
            Script.AppendLine("Result = true");
            Script.AppendLine("try {");
            Script.AppendLine(@"KNLMeansCL(device_type=""GPU"")");
            Script.AppendLine("} catch(error_msg) {");
            Script.AppendLine("Result = false");
            Script.AppendLine("}");
            Script.AppendLine(@"WriteFileStart(""{0}"", string(Result))", TempResult);
            Script.WriteToFile(TempScript);

            // Run script.
            FfmpegBusiness.Run(@"Encoder\avs2yuv.exe", String.Format(@"""{0}"" -o {1}", TempScript, TempOut), true);

            // Read frame count
            bool Result = false;
            if (File.Exists(TempResult)) {
                string FileString = File.ReadAllText(TempResult);
                try {
                    Result = bool.Parse(FileString.TrimEnd());
                }
                catch {
                    Result = false;
                }
            }

            // Delete temp files.
            File.Delete(TempScript);
            File.Delete(TempResult);
            File.Delete(TempOut); // Dummy file that received avs2yuv output.

            return Result;
        }


        public class ClipInfo {
            public float FrameRate;
            public int FrameCount;
        }

    }
}
