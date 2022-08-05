using System.Windows.Input;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <summary>
/// Represents a playlist for viewing and editing.
/// </summary>
public interface IPlaylistViewModel
{
    /// <summary>
    /// Gets or sets the name of this playlist.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the list of folders in the playlist and provides selection properties.
    /// </summary>
    ICollectionView<string> Folders { get; }

    /// <summary>
    /// Shows a folder picker and adds selected folder to the list.
    /// </summary>
    RxCommandUnit AddFolderCommand { get; }

    /// <summary>
    /// Removes selected folder from the list.
    /// </summary>
    RxCommandUnit RemoveFolderCommand { get; }
}
