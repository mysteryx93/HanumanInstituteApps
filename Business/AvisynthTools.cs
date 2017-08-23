using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.WpfCommon;

namespace Business {
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
            string TempScriptBase = PathManager.GetPathWithoutExtension(source);
            string TempScript = TempScriptBase + "_count.avs";
            string TempResult = TempScriptBase + "_count.txt";

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
        /// Saves the audio output of specified script into a WAV file.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <param name="destination">The WAV file to write.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        public static void SaveAudioToWav(MediaEncoderSettings settings, string destination, ProcessStartOptions options) {
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
            Script.AppendLine(@"TWriteWAV(""{0}"", true)", Script.GetAsciiPath(destination));
            Script.AppendLine("ForceProcessWAV()");

            // Write temp script.
            Script.WriteToFile(TempFile);
            // Execute. It aways returns an error but the file is generated.
            FFmpegProcess Worker = new FFmpegProcess(options);
            Worker.RunAvisynth(TempFile);
            File.Delete(TempFile);
        }

        /// <summary>
        /// Calculates auto-crop coordinates for specified video script.
        /// </summary>
        /// <param name="settings">The script to analyze.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>The auto-crop coordinates.</returns>
        public static Rect GetAutoCropRect(MediaEncoderSettings settings, ProcessStartOptions options) {
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
            FFmpegProcess Worker = new FFmpegProcess(options);
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
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>A float value representing the audio gain that can be applied, or null if it failed.</returns>
        public static float? GetAudioGain(MediaEncoderSettings settings, ProcessStartOptions options) {
            FFmpegProcess Worker = new FFmpegProcess(options);
            string Args = string.Format(@"-i ""{0}"" -af ""volumedetect"" -f null NUL", settings.FilePath);
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
        private static SupportedOpenClVersion TestSupportedOpenClVersion() {
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
            Worker.Options.Timeout = TimeSpan.FromSeconds(10);
            Worker.Options.DisplayMode = FFmpegDisplayMode.ErrorOnly;
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

            return Result;
        }


        /// <summary>
        /// Encodes specified video file according to settings. The script file must already be written.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <param name="frameCount">The amount of frames to process.</param>
        /// <param name="totalFrameCount">If encoding in various segments, the total amount of frames in the script.</param>
        /// <returns>The endoding completion status..</returns>
        public static CompletionStatus EncodeVideo(MediaEncoderSettings settings, long frameCount, long totalFrameCount) {
            CompletionStatus Result = CompletionStatus.None;
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
            if (settings.ParallelProcessing > 1)
                Args += " -threads 4";

            ProcessStartOptions Options = new ProcessStartOptions(settings.JobIndex, "Processing Video", true).TrackProcess(settings);
            // Options.FrameCount = AvisynthTools.GetFrameCount(settings.ScriptFile, new ProcessStartOptions(settings.JobIndex, "Getting Frame Count", false).TrackProcess(settings));
            Options.FrameCount = frameCount;
            Options.ResumePos = settings.ResumePos;
            Options.TotalFrameCount = totalFrameCount;
            if (settings.CompletionStatus == CompletionStatus.Success) {
                string JobScript = settings.OutputScriptFile;
                File.Delete(JobScript);
                File.Copy(settings.ScriptFile, JobScript);
                EditStartPosition(JobScript, settings.ResumePos, settings.ResumePos + frameCount - 1);
                Result = MediaEncoder.Encode(JobScript, Codec, null, Args, settings.OutputFile, Options);
                File.Delete(JobScript);
            } else
                Result = settings.CompletionStatus;
            return Result;
        }

        /// <summary>
        /// Tracks the process that will be run into a MediaEncoderSettings object.
        /// </summary>
        /// <param name="options">The options object used to track process status.</param>
        /// <param name="settings">The settings object that will store the process reference.</param>
        /// <returns>The options object.</returns>
        public static ProcessStartOptions TrackProcess(this ProcessStartOptions options, MediaEncoderSettings settings) {
            options.Started += (sender, e) => {
                if (settings.Processes == null)
                    settings.Processes = new List<FFmpegProcess>();
                settings.Processes.Add(e.Process);
                e.Process.Completed += (sender2, e2) => {
                    settings.CompletionStatus = e2.Status;
                    settings.Processes.Remove(e.Process);
                };
            };
            return options;
        }

        /// <summary>
        /// Encodes specified audio file according to settings. The script file must already be written.
        /// </summary>
        /// <param name="settings">An object containing the encoding settings.</param>
        /// <returns>The endoding completion status..</returns>
        public static CompletionStatus EncodeAudio(MediaEncoderSettings settings) {
            CompletionStatus Result = CompletionStatus.Success;
            string WavFile = PathManager.GetAudioFile(settings.JobIndex, AudioActions.EncodeWav);
            ProcessStartOptions Options = new ProcessStartOptions(settings.JobIndex, "Exporting Audio", false).TrackProcess(settings);
            if (!File.Exists(WavFile)) {
                AvisynthTools.SaveAudioToWav(settings, WavFile, Options);
                if (settings.CompletionStatus == CompletionStatus.Cancelled) {
                    File.Delete(WavFile);
                    return CompletionStatus.Cancelled;
                }
                if (!File.Exists(WavFile)) {
                    settings.Cancel();
                    return CompletionStatus.Error;
                }
            }

            string DestFile = PathManager.GetAudioFile(settings.JobIndex, settings.AudioAction);
            if (!File.Exists(DestFile)) {
                Options.Title = "Encoding Audio";
                if (settings.AudioAction == AudioActions.EncodeOpus) {
                    string Args = string.Format(@"--bitrate {0} ""{1}"" ""{2}""", settings.AudioQuality, WavFile, DestFile);
                    FFmpegProcess Worker = new FFmpegProcess(Options);
                    Result = Worker.Run("Encoder\\opusenc.exe", Args);
                } else if (settings.AudioAction == AudioActions.EncodeAac || settings.AudioAction == AudioActions.EncodeFlac) {
                    Result = MediaEncoder.Encode(WavFile, null,
                        settings.AudioAction == AudioActions.EncodeFlac ? "flac" : "aac",
                        string.Format("-b:a {0}k", settings.AudioQuality),
                        DestFile,
                        Options);
                }
            }

            if (Result != CompletionStatus.Success || !File.Exists(DestFile)) {
                File.Delete(DestFile);
                settings.Cancel();
            }
            return Result;
        }

        /// <summary>
        /// Edits specified script to start at specified frame position.
        /// </summary>
        /// <param name="frame">The frame position to start at.</param>
        public static void EditStartPosition(string source, long start, long end) {
            List<string> Script = File.ReadAllLines(source).ToList();
            // If the START_POSITION marker is already present, replace it, otherwise insert it.
            string Marker = "## START_POSITION ##";
            for (int i = Script.Count - 1; i >= 0; i--) {
                if (Script[i].Trim().Length > 1 && !Script[i].StartsWith("Prefetch(", StringComparison.InvariantCultureIgnoreCase)) {
                    string NewLine = string.Format("Trim({0}, {1})  {2}", start, end, Marker);
                    if (Script[i].EndsWith(Marker)) {
                        if (start > 0 || end > 0)
                            Script[i] = NewLine;
                        else
                            Script.RemoveAt(i);
                    } else if (start > 0 || end > 0)
                        Script.Insert(i + 1, NewLine);
                    break;
                }
            }
            File.WriteAllLines(source, Script);
        }
    }
}
