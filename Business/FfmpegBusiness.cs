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
                return Run(string.Format(@"-i ""{0}"" -i ""{1}"" -acodec copy -vcodec copy -map 0:v -map 1:a ""{2}""", videoFile, audioFile, destination), silent);
            else
                return Run(string.Format(@"-i ""{0}"" -vcodec copy ""{1}""", videoFile, destination), silent);
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
            bool Success = Run(string.Format(@"-i ""{0}"" -vcodec utvideo -an ""{1}""", source, destination), silent);
            if (!Success)
                File.Delete(destination);
            return Success;
        }

        /// <summary>
        /// Converts specified file into H264 format.
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="destination">The destination file, ending with .264</param>
        /// <param name="encodingQuality">The quality of the encoded x264 file, normally between 18 and 30.</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool ConvertToH264(string source, string destination, int encodingQuality) {
            File.Delete(destination);
            return RunX264(string.Format(@"--preset slower --crf {2} -o ""{0}"" ""{1}""", destination, source, encodingQuality), false);
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
            return Run(string.Format(@"-i ""{0}"" -vcodec copy -an ""{1}""", source, destination), silent);
        }

        /// <summary>
        /// Extracts the audio from specified video.
        /// </summary>
        /// <param name="source">The video to extract the audio from.</param>
        /// <param name="destination">The destination file</param>
        /// <returns>Whether the operation was completed.</returns>
        public static bool ExtractAudio(string source, string destination) {
            File.Delete(destination);
            return Run(string.Format(@"-i ""{0}"" -vn -acodec copy ""{1}""", source, destination), false);
        }

        /// <summary>
        /// Runs FFMPEG with specified arguments.
        /// </summary>
        /// <param name="arguments">FFMPEG startup arguments.</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        /// <returns>Whether the operation was completed.</returns>
        private static bool Run(string arguments, bool silent) {
            Process P = new Process();
            if (silent) {
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //P.StartInfo.RedirectStandardError = true;
                //P.StartInfo.UseShellExecute = false;
            }
            P.StartInfo.FileName = @"Encoder\ffmpeg.exe";
            P.StartInfo.Arguments = arguments;
            // P.ErrorDataReceived += P_ErrorDataReceived;
            P.Start();
            // P.BeginErrorReadLine();
            P.WaitForExit();
            // ExitCode is 0 for normal exit. Different value when closing the console.
            return P.ExitCode == 0;
        }

        /// <summary>
        /// Runs x264 with specified arguments.
        /// </summary>
        /// <param name="arguments">x264 startup arguments.</param>
        /// <param name="silent">If true, the x264 window will be hidden.</param>
        private static bool RunX264(string arguments, bool silent) {
            Process P = new Process();
            if (silent)
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            // P.StartInfo.FileName = x64 ? @"Encoder_x64\x264.exe" : @"Encoder\x264.exe";
            P.StartInfo.FileName = @"Encoder\x264.exe";
            P.StartInfo.Arguments = arguments;
            P.Start();
            if (!P.HasExited) {
                P.PriorityClass = ProcessPriorityClass.BelowNormal;
                P.WaitForExit();
            }
            return P.ExitCode == 0;
        }
    }
}
