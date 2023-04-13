
using HanumanInstitute.Avalonia;

namespace HanumanInstitute.Player432Hz.ViewModels;

/// <summary>
/// Represents a list of files to view the content of specified folders.
/// </summary>
public interface IFilesListViewModel
{
    /// <summary>
    /// Gets the list of files and selection properties.
    /// </summary>
    ICollectionView<FileItem> Files { get; }

    /// <summary>
    /// Sets the folder paths from which to load files.
    /// </summary>
    /// <param name="paths">The list of folder paths to load.</param>
    void SetPaths(IEnumerable<string>? paths);

    /// <summary>
    /// Starts playing the selected playlist. If string parameter is specified, the specified file path will be played first.
    /// </summary>
    RxCommandUnit Play { get; }
}
