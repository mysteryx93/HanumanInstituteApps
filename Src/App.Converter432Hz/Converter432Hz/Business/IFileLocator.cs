namespace HanumanInstitute.Converter432Hz.Business;

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
    IEnumerable<FileItem> GetAudioFiles(string path);

    /// <summary>
    /// Returns a list of all audio files in specified directories, searching recursively.
    /// </summary>
    /// <param name="paths">A list of paths to search for audio files.</param>
    /// <returns>A list of audio files.</returns>
    IEnumerable<FileItem> GetAudioFiles(IEnumerable<string> paths);
}
