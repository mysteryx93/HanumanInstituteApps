using System;
using System.IO;
using System.Threading;

namespace EmergenceGuardian.Avisynth {
    internal static class PathManager {
        public static readonly string[] AudioExtensions = new string[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" };

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
                    Path.ChangeExtension(dest, null),
                    DuplicateIndex > 1 ? string.Format(" ({0})", DuplicateIndex) : "",
                    Path.GetExtension(dest));
            } while (File.Exists(DestFile));
            return DestFile;
        }

        /// <summary>
        /// Returns a unique temporary file with given extension.
        /// </summary>
        /// <param name="extension">The extension of the temporary file to create.</param>
        /// <returns></returns>
        public static string GetTempFile(string extension) {
            string FileName = Guid.NewGuid().ToString() + extension;
            string TempPath = Path.Combine(Path.GetTempPath(), FileName);
            if (File.Exists(TempPath))
                return GetTempFile(extension);
            else
                return TempPath;
        }
    }
}
