using HanumanInstitute.CommonServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Provides discovery service for audio files.
    /// </summary>
    public class FileLocator : IFileLocator
    {
        private readonly IAppPathService appPath;
        private readonly IFileSystemService fileSystem;


        public FileLocator(IAppPathService appPathService, IFileSystemService fileSystemService)
        {
            appPath = appPathService;
            fileSystem = fileSystemService;
        }

        /// <summary>
        /// Returns a list of all audio files in specified directory, searching recursively.
        /// </summary>
        /// <param name="path">The path to search for audio files.</param>
        /// <returns>A list of audio files.</returns>
        public IEnumerable<string> GetAudioFiles(string path)
        {
            return fileSystem.GetFilesByExtensions(path, appPath.AudioExtensions, System.IO.SearchOption.AllDirectories);
        }

        /// <summary>
        /// Returns a list of all audio files in specified directories, searching recursively.
        /// </summary>
        /// <param name="paths">A list of paths to seasrch for audio files.</param>
        /// <returns>A list of audio files.</returns>
        public IEnumerable<string> GetAudioFiles(IEnumerable<string> paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));

            var Result = new List<string>();
            foreach (var item in paths)
            {
                Result.AddRange(GetAudioFiles(item));
            }
            return Result;
        }
    }
}
