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
        /// <param name="file1">The video file with missing audio.</param>
        /// <param name="file2">The video file containing the audio.</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        /// <param name="destination">The output file.</param>
        public static void JoinAudioVideo(string file1, string file2, string destination, bool silent) {
            File.Delete(destination);
            if (!string.IsNullOrEmpty(file2))
                Run(string.Format(@"-i ""{0}"" -i ""{1}"" -acodec copy -vcodec copy ""{2}""", file1, file2, destination), silent);
            else
                Run(string.Format(@"-i ""{0}"" -vcodec copy ""{1}""", file1, destination), silent);
        }

        /// <summary>
        /// Converts specified file into AVI format.
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="destination">The destination file, ending with .AVI</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        public static void ConvertToAVI(string source, string destination, bool silent) {
            File.Delete(destination);
            Run(string.Format(@"-i ""{0}"" -vcodec utvideo -an ""{1}""", source, destination), silent);
            // -vcodec huffyuv or utvideo, -acodec pcm_s16le
        }

        /// <summary>
        /// Converts specified file into H264 format.
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="destination">The destination file, ending with .264</param>
        /// <param name="encodingQuality">The quality of the encoded x264 file, normally between 18 and 30.</param>
        public static void ConvertToH264(string source, string destination, int encodingQuality) {
            File.Delete(destination);
            RunX264(string.Format(@"--preset slower --crf {2} -o ""{0}"" ""{1}""", destination, source, encodingQuality), false);
        }

        /// <summary>
        /// Extracts the audio from specified video.
        /// </summary>
        /// <param name="source">The video to extract the audio from.</param>
        /// <param name="destination">The destination file</param>
        public static void ExtractAudio(string source, string destination) {
            File.Delete(destination);
            Run(string.Format(@"-i ""{0}"" -vn -acodec copy ""{1}""", source, destination), false);
        }

        /// <summary>
        /// Runs FFMPEG with specified arguments.
        /// </summary>
        /// <param name="arguments">FFMPEG startup arguments.</param>
        /// <param name="silent">If true, the FFMPEG window will be hidden.</param>
        private static void Run(string arguments, bool silent) {
            Process P = new Process();
            if (silent)
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            P.StartInfo.FileName = @"Encoder\ffmpeg.exe";
            P.StartInfo.Arguments = arguments;
            P.Start();
            P.WaitForExit();
        }

        /// <summary>
        /// Runs x264 with specified arguments.
        /// </summary>
        /// <param name="arguments">x264 startup arguments.</param>
        /// <param name="silent">If true, the x264 window will be hidden.</param>
        private static void RunX264(string arguments, bool silent) {
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
        }
    }
}
