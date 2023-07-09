using System;
using System.IO.Abstractions;

namespace HanumanInstitute.AvisynthScriptBuilder {
    /// <summary>
    /// Manages the paths used by the Avisynth library.
    /// </summary>
    public class ScriptPathService : IScriptPathService {
        private readonly IFileSystem fileSystem;

        public ScriptPathService() : this (new FileSystem()) { }

        public ScriptPathService(IFileSystem fileSystemService) {
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            // PluginsPath = fileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Encoder\");
            PluginsPath = @"C:\GitHub\NaturalGroundingPlayer\Encoder\";
        }

        /// <summary>
        /// Gets or sets the path where Avisynth plugins are stored.
        /// </summary>
        public string PluginsPath { get; set; }

        /// <summary>
        /// Returns a list of all valid audio extensions.
        /// </summary>
        public string[] AudioExtensions { get; set; } = new string[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" };

        /// <summary>
        /// Gets the newline string defined for this environment.
        /// </summary>
        public string NewLine => Environment.NewLine;

        /// <summary>
        /// Returns the next available file name to avoid overriding an existing file.
        /// </summary>
        /// <param name="dest">The attempted destination.</param>
        /// <returns>The next available file name.</returns>
        public string GetNextAvailableFileName(string dest) {
            int DuplicateIndex = 0;
            string DestFile;
            do {
                DuplicateIndex++;
                DestFile = string.Format("{0}{1}{2}",
                    fileSystem.Path.ChangeExtension(dest, null),
                    DuplicateIndex > 1 ? string.Format(" ({0})", DuplicateIndex) : "",
                    fileSystem.Path.GetExtension(dest));
            } while (fileSystem.File.Exists(DestFile));
            return DestFile;
        }

        /// <summary>
        /// Returns a unique temporary file with given extension.
        /// </summary>
        /// <param name="extension">The extension of the temporary file to create.</param>
        /// <returns></returns>
        public string GetTempFile(string extension) {
            string FileName = Guid.NewGuid().ToString() + extension;
            string TempPath = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), FileName);
            if (fileSystem.File.Exists(TempPath))
                return GetTempFile(extension);
            else
                return TempPath;
        }
    }
}
