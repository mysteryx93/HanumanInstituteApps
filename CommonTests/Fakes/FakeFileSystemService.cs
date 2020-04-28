using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.CommonTests
{
    public class FakeFileSystemService : MockFileSystem, IFileSystemService
    {
        public FakeFileSystemService() { }

        public FakeFileSystemService(IDictionary<string, MockFileData> files, string currentDirectory = "") :
            base(files, currentDirectory)
        { }

        /// <summary>
        /// Ensures the directory of specified path exists. If it doesn't exist, creates the directory.
        /// </summary>
        /// <param name="path">The absolute path to validate.</param>
        public void EnsureDirectoryExists(string path)
        {
            // Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        /// <summary>
        /// Deletes a file if it exists.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        public void DeleteFileSilent(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Returns all files of specified extensions.
        /// </summary>
        /// <param name="path">The path in which to search.</param>
        /// <param name="extensions">A list of file extensions to return, each extension must include the dot.</param>
        /// <param name="searchOption">Specifies additional search options.</param>
        /// <returns>A list of files paths matching search conditions.</returns>
        public IEnumerable<string> GetFilesByExtensions(string path, IEnumerable<string> extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
            if (string.IsNullOrWhiteSpace(path)) { throw new ArgumentException(); }

            var result = new List<string>();
            try
            {
                return Directory.EnumerateFiles(path, "*", searchOption).Where(f => extensions.Any(s => f.EndsWith(s, StringComparison.InvariantCulture)));
            }
            catch (DirectoryNotFoundException) { }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }

            return result;
        }

        /// <summary>
        /// Returns specified path without its file extension.
        /// </summary>
        /// <param name="path">The path to truncate extension from.</param>
        /// <returns>A file path with no file extension.</returns>
        public string GetPathWithoutExtension(string path)
        {
            path.CheckNotNullOrEmpty(nameof(path));
            return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
        }

        /// <summary>
        /// Send a file or path silently to the recycle bin. Surpress dialog, surpress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle.</param>
        public void MoveToRecycleBin(string path) => MoveToRecycleBin(path, false);

        /// <summary>
        /// Sends a file or path to the recycle bin.
        /// </summary>
        /// <param name="displayWarning">Whether to display a warning if file is too large for the recycle bin.</param>
        /// <param name="path">Location of directory or file to recycle.</param>
        public void MoveToRecycleBin(string path, bool displayWarning)
        {
            File.Delete(path);
        }
    }
}
