using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HanumanInstitute.Player432Hz.Business;

/// <summary>
/// Provides discovery service for audio files.
/// </summary>
public class FileLocator : IFileLocator
{
    private readonly IAppPathService _appPath;
    private readonly IFileSystemService _fileSystem;

    public FileLocator(IAppPathService appPathService, IFileSystemService fileSystemService)
    {
        _appPath = appPathService;
        _fileSystem = fileSystemService;
    }

    /// <summary>
    /// Returns a list of all audio files in specified directory, searching recursively.
    /// </summary>
    /// <param name="path">The path to search for audio files.</param>
    /// <returns>A list of audio files.</returns>
    public IEnumerable<string> GetAudioFiles(string path)
    {
        return _fileSystem.GetFilesByExtensions(path, _appPath.AudioExtensions, System.IO.SearchOption.AllDirectories);
    }

    /// <summary>
    /// Returns a list of all audio files in specified directories, searching recursively.
    /// </summary>
    /// <param name="paths">A list of paths to search for audio files.</param>
    /// <returns>A list of audio files.</returns>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public IEnumerable<FileItem> GetAudioFiles(IEnumerable<string>? paths)
    {
        paths.CheckNotNull(nameof(paths));

        var result = new List<FileItem>();
        foreach (var item in paths.Select(x => _fileSystem.GetPathWithFinalSeparator(x)))
        {
            result.AddRange(GetAudioFiles(item).Select(x => 
                new FileItem(TrimPath(x, item), x)));
        }
        return result;

        // Remove load path from file path. 
        string TrimPath(string file, string path) =>
            file.StartsWith(path) ? file[path.Length..] : file;
    }
}
