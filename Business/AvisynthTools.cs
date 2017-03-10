using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EmergenceGuardian.FFmpeg;

namespace Business {
    public static class AvisynthTools {
        /// <summary>
        /// Gets an AviSynth clip information by running a script that outputs the frame count to a file.
        /// </summary>
        /// <param name="scriptFile">The AviSynth script to get information about.</param>
        /// <returns>The frame count.</returns>
        public static long GetFrameCount(string scriptFile) {
            string TempScriptBase = Path.Combine(Path.GetDirectoryName(scriptFile), Path.GetFileNameWithoutExtension(scriptFile));
            string TempScript = TempScriptBase + ".framecount.avs";
            string TempResult = TempScriptBase + ".framecount.txt";

            // Read source script and remove MT. Also remove Deshaker if present.
            string FileContent = File.ReadAllText(scriptFile);
            FileContent.Replace(Environment.NewLine + "Deshaker", Environment.NewLine + "#Deshaker");
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(FileContent);
            Script.RemoveMT();

            // Get frame count.
            Script.AppendLine();
            Script.AppendLine(@"WriteFileStart(""{0}"", ""FrameRate""{1}""Framecount"")", TempResult, @", """""" "","" """""", ");
            Script.AppendLine("Trim(0,-1)");
            Script.WriteToFile(TempScript);

            // Run script.
            FFmpegProcess Worker = new FFmpegProcess();
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
        /// Saves the audio output of specified script into a WAV file.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <param name="displayMode">How to display the task while it is running.</param>
        public static void SaveAudioToWav(MediaEncoderSettings settings, FFmpegDisplayMode displayMode) {
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
            // Execute. It aways returns an error but the file is generated.
            FFmpegProcess Worker = new FFmpegProcess(new ProcessStartOptions(displayMode, "Extracting Audio"));
            Worker.RunAvisynth(TempFile);
            File.Delete(TempFile);
        }

        /// <summary>
        /// Calculates auto-crop coordinates for specified video script.
        /// </summary>
        /// <param name="settings">The script to analyze.</param>
        /// <returns>The auto-crop coordinates.</returns>
        public static Rect GetAutoCropRect(MediaEncoderSettings settings) {
            string TempScript = settings.TempFile + ".avs";
            string TempResult = settings.TempFile + ".txt";

            // Create script to get auto-crop coordinates
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            Script.OpenDirect(settings.FilePath, false);
            Script.LoadPluginDll("RoboCrop26.dll");
            Script.AppendLine(@"RoboCrop(LogFn=""{0}"")", Script.GetAsciiPath(TempResult));
            Script.AppendLine("Trim(0,-1)");
            Script.WriteToFile(TempScript);

            // Run script.
            FFmpegProcess Worker = new FFmpegProcess();
            Worker.RunAvisynth(TempScript);

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
        /// Returns the audio gain that can be applied to an audio file.
        /// </summary>
        /// <param name="settings">The settings pointing to the file to analyze.</param>
        /// <returns>A float value representing the audio gain that can be applied, or null if it failed.</returns>
        public static float? GetAudioGain(MediaEncoderSettings settings) {
            string TempResult = settings.TempFile + ".txt";
            FFmpegProcess Worker = new FFmpegProcess();
            string Args = string.Format(@"-af ""volumedetect"" -f null null > ""{0}"" 2>&1", TempResult);
            Worker.RunAvisynthToFFmpeg(settings.FilePath, Args);
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

        /// <summary>
        /// The first time the media encoder window is shown, it will test whether OpenCL 1.2 is support to use the latest version 
        /// of KNLMeans. If not, an older version of the library will be used.
        /// </summary>
        private static SupportedOpenClVersion gpuSupportsOpenCL12;
        public static SupportedOpenClVersion GpuSupport {
            get {
                if (gpuSupportsOpenCL12 == SupportedOpenClVersion.NotTested) {
                    gpuSupportsOpenCL12 = TestSupportedOpenClVersion();
                }
                return gpuSupportsOpenCL12;
            }
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
            //string TempOut = Settings.TempFilesPath + "Temp.y4m";

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
            FFmpegProcess Worker = new FFmpegProcess();
            Worker.RunAvisynth(TempScript);

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
            //File.Delete(TempOut); // Dummy file that received avs2yuv output.

            return Result;
        }


        /// <summary>
        /// Encodes specified video file according to settings. The script file must already be written.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <returns>The endoding completion status..</returns>
        public static CompletionStatus EncodeVideo(MediaEncoderSettings settings) {
            File.Delete(settings.OutputFile);
            string Codec = "", Args = "";
            if (settings.VideoCodec == VideoCodecs.x264) {
                Codec = "libx264";
                Args = string.Format("-psy-rd 1:0.05 -preset {0} -crf {1}", settings.EncodePreset, settings.EncodeQuality);
            } else if (settings.VideoCodec == VideoCodecs.x265) {
                Codec = "libx265";
                Args = string.Format("-preset {0} -crf {1}", settings.EncodePreset, settings.EncodeQuality);
            } else // AVI
                Codec = "utvideo";

            ProcessStartOptions Options = new ProcessStartOptions(FFmpegDisplayMode.Interface, "Processing Video");
            Options.FrameCount = AvisynthTools.GetFrameCount(settings.ScriptFile);
            return MediaEncoder.Encode(settings.ScriptFile, Codec, null, Args, settings.OutputFile, Options);
        }

        /// <summary>
        /// Encodes specified audio file according to settings. The script file must already be written.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <returns>The endoding completion status..</returns>
        public static CompletionStatus EncodeAudio(MediaEncoderSettings settings) {
            FFmpegDisplayMode DisplayMode = settings.VideoCodec == VideoCodecs.Copy ? FFmpegDisplayMode.Interface : FFmpegDisplayMode.ErrorOnly;
            AvisynthTools.SaveAudioToWav(settings, DisplayMode);

            ProcessStartOptions Options = new ProcessStartOptions(DisplayMode, "Encoding Audio");
            if (settings.AudioAction == AudioActions.EncodeOpus) {
                string Args = string.Format(@"--bitrate {0} ""{1}"" ""{2}""", settings.AudioQuality, settings.AudioFileWav, settings.AudioFileOpus);
                FFmpegProcess Worker = new FFmpegProcess(Options);
                return Worker.Run("Encoder\\opusenc.exe", Args, false);
            } else if (settings.AudioAction == AudioActions.EncodeAac || settings.AudioAction == AudioActions.EncodeFlac) {
                return MediaEncoder.Encode(settings.AudioFileWav, null,
                    settings.AudioAction == AudioActions.EncodeFlac ? "flac" : "",
                    string.Format("-b:a {1}k", settings.AudioQuality),
                    settings.AudioAction == AudioActions.EncodeFlac ? settings.AudioFileFlac : settings.AudioFileAac,
                    Options);
            } else
                return CompletionStatus.Success;
        }
    }
}
