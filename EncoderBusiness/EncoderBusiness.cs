using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.Avisynth;

namespace EmergenceGuardian.MediaEncoder {
    public static class EncoderBusiness {
        /// <summary>
        /// Calculates auto-crop coordinates for specified video script.
        /// </summary>
        /// <param name="filePath">The script to analyze.</param>
        /// <param name="sourceWidth">The width of the source video.</param>
        /// <param name="sourceHeight">The height of the source video.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>The auto-crop coordinates.</returns>
        public static Rect GetAutoCropRect(string filePath, int sourceWidth, int sourceHeight, ProcessStartOptions options) {
            string TempScript = PathManager.GetTempFile(".avs");
            string TempResult = Path.ChangeExtension(TempScript, ".txt");

            // Create script to get auto-crop coordinates
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            Script.OpenDirect(filePath, false);
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
                    Result.Left = Math.Max(int.Parse(Values[10]), 0);
                    Result.Top = Math.Max(int.Parse(Values[11]), 0);
                    Result.Right = Math.Max(sourceWidth - int.Parse(Values[12]), 0);
                    Result.Bottom = Math.Max(sourceHeight - int.Parse(Values[13]), 0);
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
                } catch (Exception e) {
                    LastError = e;
                    System.Threading.Thread.Sleep(500);
                }
            }
            if (LastError != null)
                throw LastError;

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
            if (settings.VideoAction != VideoAction.Copy) {
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
            if (settings.VideoAction == VideoAction.x264) {
                Codec = "libx264";
            } else if (settings.VideoAction == VideoAction.x265) {
                Codec = "libx265";
                Args = string.Format("-preset {0} -crf {1}", settings.EncodePreset, settings.EncodeQuality);
            } else if (settings.VideoAction == VideoAction.Avi)
                Codec = "huffyuv";
            else if (settings.VideoAction == VideoAction.AviUtVideo)
                Codec = "utvideo";
            else if (settings.VideoAction == VideoAction.xvid)
                Codec = "xvid";
            else if (settings.VideoAction == VideoAction.x264_10bit)
                Args = string.Format("--preset {0} --crf {1}", settings.EncodePreset, settings.EncodeQuality);

            if (settings.ParallelProcessing > 1)
                Args += settings.VideoAction == VideoAction.x264_10bit ? " --threads 4" : " -threads 4";

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
                if (settings.VideoAction == VideoAction.x264_10bit)
                    Result = EncodeX264_10bit(JobScript, Args, settings.OutputFile, Options);
                else
                    Result = FFmpeg.MediaEncoder.Encode(JobScript, Codec, null, Args, settings.OutputFile, Options);
                File.Delete(JobScript);
            } else
                Result = settings.CompletionStatus;
            return Result;
        }

        /// <summary>
        /// Encodes an Avisynth script through X264 10bit.
        /// </summary>
        /// <param name="source">The script to encode.</param>
        /// <param name="encodeArgs">Options for x264.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns></returns>
        public static CompletionStatus EncodeX264_10bit(string source, string encodeArgs, string destination, ProcessStartOptions options) {
            File.Delete(destination);
            StringBuilder Query = new StringBuilder("--demuxer y4m ");
            Query.Append(encodeArgs);
            Query.Append(" -o \"");
            Query.Append(destination);
            Query.Append("\" -");

            // Run x264 with query.
            FFmpegProcess Worker = new FFmpegProcess(options);
            CompletionStatus Result = Worker.RunAvisynthToEncoder(source, Query.ToString(), EncoderApp.x264, AppPaths.X264Path);
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
            string WavFile = PathManager.GetAudioFile(settings.JobIndex, AudioActions.Wav);
            ProcessStartOptions Options = new ProcessStartOptions(settings.JobIndex, "Exporting Audio", false).TrackProcess(settings);
            if (!File.Exists(WavFile)) {
                EncoderBusiness.SaveAudioToWav(settings, WavFile, Options);
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
                if (settings.AudioAction == AudioActions.Opus) {
                    string Args = string.Format(@"--bitrate {0} ""{1}"" ""{2}""", settings.AudioQuality, WavFile, DestFile);
                    FFmpegProcess Worker = new FFmpegProcess(Options);
                    Result = Worker.Run("Encoder\\opusenc.exe", Args);
                } else if (settings.AudioAction == AudioActions.Aac || settings.AudioAction == AudioActions.Flac) {
                    Result = FFmpeg.MediaEncoder.Encode(WavFile, null,
                        settings.AudioAction == AudioActions.Flac ? "flac" : "aac",
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
