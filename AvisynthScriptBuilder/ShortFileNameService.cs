using System;
using System.IO.Abstractions;
using System.Text;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Gives access to DOS short path names.
    /// </summary>
    public class ShortFileNameService : IShortFileNameService
    {
        private readonly IScriptPathService avsPath;
        private readonly IFileSystem fileSystem;
        private readonly IWindowsApiService windowsApi;

        public ShortFileNameService() : this(new ScriptPathService(), new FileSystem(), new WindowsApiService()) { }

        public ShortFileNameService(IScriptPathService avsPathService, IFileSystem fileSystemService, IWindowsApiService windowsApiService)
        {
            this.avsPath = avsPathService ?? throw new ArgumentNullException(nameof(avsPathService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.windowsApi = windowsApiService ?? throw new ArgumentNullException(nameof(windowsApiService));
        }

        private string GetShortPath(string path)
        {
            string Result = windowsApi.GetShortPathName(path);
            if (path.Equals(Result))
            {
                // Short path wasn't generated, make sure option is enabled on drive.
                if (EnableShortNameGeneration(path))
                {
                    // Only way to generate short file name is to copy file and delete original.
                    string PathCopy = avsPath.GetNextAvailableFileName(path);
                    fileSystem.File.Copy(path, PathCopy);
                    if (fileSystem.File.Exists(PathCopy))
                    {
                        fileSystem.File.Delete(path);
                        fileSystem.File.Move(PathCopy, path);
                        // Read generated short path.
                        Result = GetShortPath(path);
                    }
                }
            }
            return Result;
        }

        public bool EnableShortNameGeneration(string path)
        {
            string Drive = null;
            if (path.Length > 3 && path[1] == ':')
                Drive = path.Substring(0, 2);
            return true;
            // fsutil 8dot3name query e:
            // fsutil behavior set disable8dot3 e: 0
        }

        /// <summary>
        /// Returns whether specified value contains only ASCII characters.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>True if value contains only ASCII characters, otherwise false.</returns>
        public bool IsASCII(string value)
        {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        /// <summary>
        /// Returns the short path name (DOS) for specified path.
        /// </summary>
        /// <param name="path">The path for which to get the short path.</param>
        /// <returns>The short path.</returns>
        public string GetAsciiPath(string path)
        {
            if (!IsASCII(path))
                return GetShortPath(path);
            else
                return path;
        }
    }
}
