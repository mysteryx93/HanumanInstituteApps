using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Extends IFileSystem with a few extra IO functions. IFileSystem provides wrappers around all IO methods.
    /// </summary>
    public interface IFileSystemService : IFileSystem
    {
        /// <summary>
        /// Ensures the directory of specified path exists. If it doesn't exist, creates the directory.
        /// </summary>
        /// <param name="path">The absolute path to validate.</param>
        void EnsureDirectoryExists(string path);
        /// <summary>
        /// Deletes a file if it exists.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        void DeleteFileSilent(string path);
        /// <summary>
        /// Returns all files of specified extensions.
        /// </summary>
        /// <param name="path">The path in which to search.</param>
        /// <param name="extensions">A list of file extensions to return, each extension must include the dot.</param>
        /// <param name="searchOption">Specifies additional search options.</param>
        /// <returns>A list of files paths matching search conditions.</returns>
        IEnumerable<string> GetFilesByExtensions(string path, IEnumerable<string> extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly);
        /// <summary>
        /// Returns specified path without its file extension.
        /// </summary>
        /// <param name="path">The path to truncate extension from.</param>
        /// <returns>A file path with no file extension.</returns>
        string GetPathWithoutExtension(string path);
        /// <summary>
        /// Send a file or path silently to the recycle bin. Surpress dialog, surpress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle.</param>
        void MoveToRecycleBin(string path);
        /// <summary>
        /// Sends a file or path to the recycle bin.
        /// </summary>
        /// <param name="displayWarning">Whether to display a warning if file is too large for the recycle bin.</param>
        /// <param name="path">Location of directory or file to recycle.</param>
        void MoveToRecycleBin(string path, bool displayWarning);
    }
}
