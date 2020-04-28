using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using HanumanInstitute.CommonServices.Properties;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Extends FileSystem with a few extra IO functions. FileSystem provides wrappers around all IO methods.
    /// </summary>
    public class FileSystemService : IFileSystemService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IWindowsApiService _windowsApi;

        //public FileSystemService() : this(new FileSystem(), new WindowsApiService()) { }

        public FileSystemService(IFileSystem fileSystemService, IWindowsApiService windowsApiService)
        {
            _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            _windowsApi = windowsApiService ?? throw new ArgumentNullException(nameof(windowsApiService));
        }

        public IDirectory Directory => _fileSystem.Directory;
        public IDirectoryInfoFactory DirectoryInfo => _fileSystem.DirectoryInfo;
        public IDriveInfoFactory DriveInfo => _fileSystem.DriveInfo;
        public IFile File => _fileSystem.File;
        public IFileInfoFactory FileInfo => _fileSystem.FileInfo;
        public IFileStreamFactory FileStream => _fileSystem.FileStream;
        public IFileSystemWatcherFactory FileSystemWatcher => _fileSystem.FileSystemWatcher;
        public IPath Path => _fileSystem.Path;

        /// <summary>
        /// Ensures the directory of specified path exists. If it doesn't exist, creates the directory.
        /// </summary>
        /// <param name="path">The absolute path to validate.</param>
        public void EnsureDirectoryExists(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
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
            if (string.IsNullOrWhiteSpace(path)) { throw new ArgumentException(Resources.ValueIsNullOrWhiteSpace, nameof(path)); }

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
            var flags = ApiFileOperationFlags.AllowUndo | ApiFileOperationFlags.NoConfirmation;
            flags |= displayWarning ? ApiFileOperationFlags.WantNukeWarning : ApiFileOperationFlags.NoErrorUI | ApiFileOperationFlags.Silent;
            _windowsApi.SHFileOperation(ApiFileOperationType.Delete, path, flags);
            if (File.Exists(path))
            {
                throw new IOException(string.Format(CultureInfo.InvariantCulture, Resources.CannotDeleteFile, path));
            }
        }
    }
}
