using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace Business {
    public static class PathManager {
        public static string PreviewScriptFile {
            get { return Settings.TempFilesPath + "Preview.avs"; }
        }
        public static string PreviewSettingsFile {
            get { return Settings.TempFilesPath + "Preview.xml"; }
        }
        public static string PreviewSourceFile {
            get { return Settings.TempFilesPath + "Preview.avi"; }
        }
        public static string PreviewDeshakerScript {
            get { return Settings.TempFilesPath + "Preview_Deshaker.avs"; }
        }
        public static string PreviewDeshakerTempOut {
            get { return Settings.TempFilesPath + "Preview_Deshaker.avi"; }
        }
        public static string PreviewDeshakerTempLog {
            get { return Settings.TempFilesPath + "Preview_Deshaker.txt"; }
        }
        public static string PreviewDeshakerLog {
            get { return Settings.TempFilesPath + "Preview_Deshaker.log"; }
        }

        public static string GetScriptFile(int jobIndex, long resumePos) {
            if (jobIndex >= 0) {
                if (resumePos > -1)
                    return string.Format("{0}Job{1}_Script_{2}.avs", Settings.TempFilesPath, jobIndex, resumePos);
                else
                    return string.Format("{0}Job{1}_Script.avs", Settings.TempFilesPath, jobIndex);
            } else
                return PreviewScriptFile;
        }

        public static string GetSettingsFile(int jobIndex) {
            if (jobIndex >= 0)
                return string.Format("{0}Job{1}_Settings.xml", Settings.TempFilesPath, jobIndex);
            else
                return PreviewSettingsFile;
        }

        public static string GetInputFile(int jobIndex) {
            if (jobIndex >= 0)
                return string.Format("{0}Job{1}_Input.avi", Settings.TempFilesPath, jobIndex);
            else
                return PreviewSourceFile;
        }

        public static string GetOutputFile(int jobIndex, long resumePos, VideoCodecs codec) {
            if (resumePos > -1)
                return string.Format("{0}Job{1}_Output_{2}.{3}", Settings.TempFilesPath, jobIndex, resumePos, codec == VideoCodecs.Avi ? "avi" : "mkv");
            else
                return string.Format("{0}Job{1}_Output.{3}", Settings.TempFilesPath, jobIndex, resumePos, codec == VideoCodecs.Avi ? "avi" : codec == VideoCodecs.Copy ? "mkv" : "mp4");
        }

        public static string GetAudioFile(int jobIndex, AudioActions codec) {
            return string.Format("{0}Job{1}_Output.{2}", Settings.TempFilesPath, jobIndex, GetAudioFileExtension(codec));
        }

        public static string GetAudioFileExtension(AudioActions codec) {
            if (codec == AudioActions.EncodeAac)
                return "aac";
            else if (codec == AudioActions.EncodeFlac)
                return "flac";
            else if (codec == AudioActions.EncodeOpus)
                return "opus";
            else
                return "wav";
        }

        public static string GetFinalFile(int jobIndex, VideoFormats codec) {
            return string.Format("{0}Job{1}_Final.{2}", Settings.TempFilesPath, jobIndex, GetFileExtension(codec));
        }

        public static string GetFileExtension(VideoFormats codec) {
            return codec == VideoFormats.Mp4 ? "mp4" : "mkv";
        }


        public static string GetTempFile(int jobIndex) {
            if (jobIndex >= 0)
                return string.Format("{0}Job{1}_Temp", Settings.TempFilesPath, jobIndex);
            else
                return string.Format("{0}Preview_Temp", Settings.TempFilesPath);
        }

        public static string GetDeshakerScript(int jobIndex) {
            if (jobIndex >= 0)
                return string.Format("{0}Job{1}_Deshaker.avs", Settings.TempFilesPath, jobIndex);
            else
                return PreviewDeshakerScript;
        }

        public static string GetDeshakerTempOut(int jobIndex) {
            if (jobIndex >= 0)
                return string.Format("{0}Job{1}_Deshaker.avi", Settings.TempFilesPath, jobIndex);
            else
                return PreviewDeshakerTempOut;
        }

        public static string GetDeshakerTempLog(int jobIndex) {
            if (jobIndex >= 0)
                return string.Format("{0}Job{1}_Deshaker.txt", Settings.TempFilesPath, jobIndex);
            else
                return PreviewDeshakerTempLog;
        }

        public static string GetDeshakerLog(int jobIndex) {
            if (jobIndex >= 0)
                return string.Format("{0}Job{1}_Deshaker.log", Settings.TempFilesPath, jobIndex);
            else
                return PreviewDeshakerLog;
        }

        public static void DeleteJobFiles(int jobIndex) {
            foreach (string f in Directory.EnumerateFiles(Settings.TempFilesPath, string.Format("Job{0}_*", jobIndex))) {
                File.Delete(f);
            }
        }

        public static void DeleteOutputFiles(int jobIndex) {
            foreach (string f in Directory.EnumerateFiles(Settings.TempFilesPath, string.Format("Job{0}_Output*", jobIndex))) {
                File.Delete(f);
            }
            foreach (string f in Directory.EnumerateFiles(Settings.TempFilesPath, string.Format("Job{0}_Final*", jobIndex))) {
                File.Delete(f);
            }
        }

        /// <summary>
        /// Moves specified file to specified destination, numerating the destination to avoid duplicates and attempting several times.
        /// </summary>
        /// <param name="source">The file to move.</param>
        /// <param name="dest">The destination to move the file to.</param>
        /// <returns>The file name being moved to.</returns>
        public static string SafeMove(string source, string dest) {
            string DestFile = GetNextAvailableFileName(dest);
            // Attempt to copy file until it becomes available.
            for (int i = 0; i < 5; i++) {
                try {
                    File.Move(source, DestFile);
                    return DestFile;
                }
                catch {
                }
                System.Threading.Thread.Sleep(500);
            }
            throw new IOException(string.Format("Cannot move file '{0}' because it is being used by another process.", source));
        }

        /// <summary>
        /// Deletes specified file and keep trying for 10 seconds without blocking executing code.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        public static void SafeDelete(string file) {
            try {
                File.Delete(file);
            } catch {
                Thread DeleteThread = new Thread(() => {
                    System.Threading.Thread.Sleep(2000);
                    for (int i = 0; i < 5; i++) {
                        try {
                            File.Delete(file);
                            return;
                        }
                        catch {}
                    }
                });
                DeleteThread.IsBackground = true;
                DeleteThread.Start();
            }
        }

        /// <summary>
        /// Returns the next available file name to avoid overriding an existing file.
        /// </summary>
        /// <param name="dest">The attempted destination.</param>
        /// <returns>The next available file name.</returns>
        public static string GetNextAvailableFileName(string dest) {
            int DuplicateIndex = 0;
            string DestFile;
            do {
                DuplicateIndex++;
                DestFile = string.Format("{0}{1}{2}",
                    PathManager.GetPathWithoutExtension(dest),
                    DuplicateIndex > 1 ? string.Format(" ({0})", DuplicateIndex) : "",
                    Path.GetExtension(dest));
            } while (File.Exists(DestFile));
            return DestFile;
        }

        /// <summary>
        /// Clears the temp folder (unfinished downloads) except Media Encoder files.
        /// </summary>
        public static void ClearTempFolder() {
            if (Settings.SavedFile == null || !Directory.Exists(Settings.TempFilesPath))
                return;

            string FileName;
            foreach (string item in Directory.EnumerateFiles(Settings.TempFilesPath)) {
                FileName = Path.GetFileName(item);
                if (!FileName.StartsWith("Preview.") && !FileName.StartsWith("Job")) {
                    try {
                        File.Delete(item);
                    }
                    catch {
                    }
                }
            }
        }

        /// <summary>
        /// Returns a file path without extension. In contrast to GetFileNameWithoutExtension, this keeps the folder names.
        /// </summary>
        /// <param name="path">A file path.</param>
        /// <returns>The path without extension.</returns>
        public static string GetPathWithoutExtension(string path) {
            int Pos = path.LastIndexOf('.');
            return Pos == -1 ? path : path.Substring(0, Pos);
        }
    }
}
