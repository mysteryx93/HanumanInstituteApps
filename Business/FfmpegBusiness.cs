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
            bool Result = true;
            File.Delete(destination);
            if (!string.IsNullOrEmpty(audioFile)) {
                // FFMPEG fails to muxe H264 into MKV container. Converting to MP4 and then muxing with the audio, however, works.
                string OriginalVideoFile = videoFile;
                if (videoFile.EndsWith(".264") && destination.EndsWith(".mkv")) {
                    videoFile = videoFile.Substring(0, videoFile.Length - 4) + ".mp4";
                    Result = JoinAudioVideo(OriginalVideoFile, null, videoFile, silent);
                }
                // Join audio and video files.
                if (Result)
                    Result = RunFfmpeg(string.Format(@"-i ""{0}"" -i ""{1}"" -acodec copy -vcodec copy -map 0:v -map 1:a ""{2}""", videoFile, audioFile, destination), silent);
                // Delete temp file.
                if (OriginalVideoFile != videoFile)
                    File.Delete(videoFile);
                return Result;
            } else
                Result = RunFfmpeg(string.Format(@"-i ""{0}"" -vcodec copy ""{1}""", videoFile, destination), silent);
            return Result;
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
            bool Success = RunFfmpeg(string.Format(@"-i ""{0}"" -vcodec utvideo -an ""{1}""", source, destination), silent);
            if (!Success)
                File.Delete(destination);
            return Success;
        }

        /// <summary>
        /// Converts specified file into H264 format.
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="destination">The destination file, ending with .mp4</param>
        /// <param name="encodeQuality">The quality of the encoded x264 file, normally between 18 and 30.</param>
        /// <param name="encodePreset">The preset used during encoding. Slower gives smaller files.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool ConvertToH264(string source, string destination, int encodeQuality, EncodePresets encodePreset) {
            File.Delete(destination);
            // return RunX264(string.Format(@"--preset veryslow --crf {2} -o ""{0}"" ""{1}""", destination, source, encodingQuality), false);
            return RunX264Pipe(string.Format(@"--preset {0} --crf {1} -o ""{2}""", encodePreset, encodeQuality, destination), source, false);
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
        /// Runs x264 with specified arguments.
        /// </summary>
        /// <param name="arguments">x264 startup arguments.</param>
        /// <param name="silent">If true, the x264 window will be hidden.</param>
        private static bool RunX264(string arguments, string source, bool silent) {
            return Run(@"Encoder\x264.exe", arguments + " \"" + source + "\"", silent, ProcessPriorityClass.BelowNormal);
        }

        /// <summary>
        /// Runs x264 through a Avs2uuv pipe to allow AviSynth and x264.exe to have separate memory spaces (each limited to 2GB).
        /// </summary>
        /// <param name="arguments">x264 startup arguments.</param>
        /// <param name="silent">If true, the x264 window will be hidden.</param>
        private static bool RunX264Pipe(string arguments, string source, bool silent) {
            int FrameCount = GetFrameCount(source, true);
            string PipeArgs = string.Format(@"/c Encoder\avs2yuv.exe ""{0}"" -o - | Encoder\x264.exe - {1} --demuxer y4m --frames {2}", source, arguments, FrameCount);
            return Run("cmd", PipeArgs, silent, ProcessPriorityClass.BelowNormal);
        }

        /// <summary>
        /// Gets the total count of frames in an AviSynth script by running a script that outputs the frame count to a file.
        /// </summary>
        /// <param name="source">The AviSynth script to get frame count for.</param>
        /// <param name="silent">If true, the x264 window will be hidden.</param>
        /// <returns>The video frame count.</returns>
        private static int GetFrameCount(string source, bool silent) {
            string TempScript = Settings.TempFilesPath + "Framecount.avs";
            string TempResult = Settings.TempFilesPath + "Framecount.txt";

            // Create script to get frame count.
            StringBuilder FrameCountScript = new StringBuilder();
            FrameCountScript.AppendFormat(@"AviSource(""{0}"")", source);
            FrameCountScript.AppendLine(@"WriteFileStart(""Framecount.txt"", ""Framecount()"")");
            FrameCountScript.AppendLine("Trim(0,-1)");
            File.WriteAllText(TempScript, FrameCountScript.ToString());

            // Run script.
            Run(@"Encoder\avs2yuv.exe", String.Format(@"""{0}"" -o Framecount.y4m", TempScript), true);

            // Read frame count
            int FrameCount = 0;
            if (File.Exists(TempResult)) {
                string FrameCountString = File.ReadAllText(TempResult);
                Int32.TryParse(FrameCountString, out FrameCount);
            }

            // Delete temp files.
            File.Delete(TempScript);
            File.Delete(TempResult);
            File.Delete(Settings.TempFilesPath + "Framecount.y4m"); // Dummy file that received avs2yuv output.

            return FrameCount;
        }
    }
}
