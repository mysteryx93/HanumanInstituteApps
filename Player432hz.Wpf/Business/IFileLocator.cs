using System;
using System.Collections.Generic;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Provides discovery service for audio files.
    /// </summary>
    public interface IFileLocator
    {
        /// <summary>
        /// Returns a list of all audio files in specified directory, searching recursively.
        /// </summary>
        /// <param name="path">The path to search for audio files.</param>
        /// <returns>A list of audio files.</returns>
        IEnumerable<string> GetAudioFiles(string path);

        /// <summary>
        /// Returns a list of all audio files in specified directories, searching recursively.
        /// </summary>
        /// <param name="paths">A list of paths to seasrch for audio files.</param>
        /// <returns>A list of audio files.</returns>
        IEnumerable<string> GetAudioFiles(IEnumerable<string> paths);
    }
}
