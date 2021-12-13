using System.Collections.Generic;
using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Player432hz.Business;

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
    public IEnumerable<string> GetAudioFiles(IEnumerable<string> paths)
    {
        paths.CheckNotNull(nameof(paths));

        var result = new List<string>();
        foreach (var item in paths)
        {
            result.AddRange(GetAudioFiles(item));
        }
        return result;
    }
}
