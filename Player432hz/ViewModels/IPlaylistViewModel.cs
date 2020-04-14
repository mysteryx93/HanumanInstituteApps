using System.Windows.Input;
using HanumanInstitute.CommonUI;

namespace HanumanInstitute.Player432hz.ViewModels
{
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
        SelectableList<string> Folders { get; set; }

        /// <summary>
        /// Shows a folder picker and adds selected folder to the list.
        /// </summary>
        ICommand AddFolderCommand { get; }

        /// <summary>
        /// Removes selected folder from the list.
        /// </summary>
        ICommand RemoveFolderCommand { get; }
    }
}