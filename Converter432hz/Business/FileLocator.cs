using System.IO;
using System.Linq;

namespace HanumanInstitute.Converter432hz.Business;

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
    public IEnumerable<FileItem> GetAudioFiles(string path)
    {
        path = _fileSystem.GetPathWithFinalSeparator(path);
        return _fileSystem.GetFilesByExtensions(path, _appPath.AudioExtensions, SearchOption.AllDirectories).Select(x => 
            new FileItem(x, TrimPath(x, path)));
    }

    /// <summary>
    /// Returns a list of all audio files in specified directories, searching recursively.
    /// </summary>
    /// <param name="paths">A list of paths to search for audio files.</param>
    /// <returns>A list of audio files.</returns>
    public IEnumerable<FileItem> GetAudioFiles(IEnumerable<string>? paths)
    {
        paths.CheckNotNull(nameof(paths));

        var result = new List<FileItem>();
        foreach (var item in paths)
        {
            result.AddRange(GetAudioFiles(item));
        }
        return result;

    }

    // Remove load path from file path. 
    private string TrimPath(string file, string path) =>
        file.StartsWith(path) ? file[path.Length..] : file;
}
