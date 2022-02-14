using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs;

namespace HanumanInstitute.Common.Services;

/// <summary>
/// Scans paths to detect broken paths, in which case it will prompt the user to locate the folders, and auto-fix the paths. 
/// </summary>
public abstract class PathFixerBase : IPathFixer
{
    private readonly IFileSystemService _fileSystem;
    private readonly IDialogService _dialogService;

    /// <summary>
    /// Initializes a new instance of the FolderPathFixerBase class.
    /// </summary>
    /// <param name="fileSystem">The service providing access to the file system.</param>
    /// <param name="dialogService">The service providing window and dialog interactions.</param>
    protected PathFixerBase(IFileSystemService fileSystem, IDialogService dialogService)
    {
        _fileSystem = fileSystem;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Scans specified list of folders to detect any missing folder.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to<see cref="IDialogService"/>.</param>
    /// <param name="folders">The list of folders to examine. Do not include file names.</param>
    public async Task<bool> ScanAndFixFoldersAsync(INotifyPropertyChanged owner, IList<string> folders)
    {
        var invalidFolder = folders.FirstOrDefault(x => !_fileSystem.Directory.Exists(x));
        if (invalidFolder != null)
        {
            invalidFolder = _fileSystem.Path.TrimEndingDirectorySeparator(invalidFolder);
            var folderName = _fileSystem.Path.GetFileName(invalidFolder);

            // Prompt whether to fix.
            var message = $"Cannot find folder \"{invalidFolder}\". It may have been moved or deleted.\n" +
                          "Do you want to fix the paths?\n\n" +
                          $"Click Yes to browse for folder \"{folderName}\".";

            var result = await _dialogService.ShowMessageBoxAsync(owner,
                message.Replace("\n", Environment.NewLine),
                "Invalid Folder Path",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning).ConfigureAwait(true);

            if (result == true)
            {
                return await BrowseNewPathAsync(owner, folders, invalidFolder, folderName).ConfigureAwait(true);
            }
        }
        return false;
    }

    /// <summary>
    /// Prompts the user to locate a missing folder.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to<see cref="IDialogService"/>.</param>
    /// <param name="folders">The list of folders to examine. Do not include file names.</param>
    /// <param name="invalidFolder">The invalid folder path to prompt for.</param>
    /// <param name="folderName">The name of the invalid folder, without its parent path.</param>
    private async Task<bool> BrowseNewPathAsync(INotifyPropertyChanged owner, IList<string> folders, string invalidFolder, string folderName)
    {
        // Browse for new path.
        var selectedPath = await _dialogService.ShowOpenFolderDialogAsync(owner).ConfigureAwait(true);
        if (selectedPath != null)
        {
            selectedPath = _fileSystem.Path.TrimEndingDirectorySeparator(selectedPath);
            var newPath = ProbeNewPath(selectedPath, folderName);
            if (newPath != null)
            {
                CalculateReplacement(invalidFolder, newPath);
                await ScanAndFixFoldersAsync(owner, folders).ConfigureAwait(true);
                return true;
            }
            else
            {
                var message = $"Folder \"{folderName}\" not found at \"{selectedPath}\".\nWould you like to try again?";

                var result = await _dialogService.ShowMessageBoxAsync(owner,
                    message.Replace("\n", Environment.NewLine),
                    "Invalid Folder Path",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning).ConfigureAwait(true);

                if (result == true)
                {
                    return await BrowseNewPathAsync(owner, folders, invalidFolder, folderName).ConfigureAwait(true);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns whether the folder is found at specific path.
    /// </summary>
    /// <param name="selectedPath">The new path to look at.</param>
    /// <param name="folderName">The folder to look for.</param>
    /// <returns>The new folder path, if found.</returns>
    private string? ProbeNewPath(string selectedPath, string folderName)
    {
        if (selectedPath.EndsWith(_fileSystem.Path.DirectorySeparatorChar + folderName))
        {
            return selectedPath;
        }
        if (_fileSystem.Directory.Exists(_fileSystem.Path.Combine(selectedPath, folderName)))
        {
            return _fileSystem.Path.Combine(selectedPath, folderName);
        }
        return null;
    }

    /// <summary>
    /// Calculates the replaceOld and replaceNew values for specified oldFolder and newFolder values.
    /// </summary>
    /// <param name="oldFolder">The old folder path.</param>
    /// <param name="newFolder">The new folder path.</param>
    private void CalculateReplacement(string oldFolder, string newFolder)
    {
        // Old folder: /Backup/Home/Music/INNA/Party
        // New folder: /Sync/Music/INNA/Party
        // Old file: /Backup/Home/Music/Other.mp3
        // New file: /Sync/Music/Other.mp3"
        // Replace "/Backup/Home/" with "Sync"

        var separator = _fileSystem.Path.DirectorySeparatorChar;
        var oldFolderParts = oldFolder.Split(separator);
        var newFolderParts = newFolder.Split(separator);
        // Count how many parts are equal starting from the end.
        var partsCount = Math.Min(oldFolderParts.Length, newFolderParts.Length);
        var equalParts = 0;
        for (var i = 1; i <= partsCount; i++)
        {
            if (oldFolderParts[^i] == newFolderParts[^i])
            {
                equalParts++;
            }
            else
            {
                break;
            }
        }
        // Get path starts to replace.
        var replaceOldCount = oldFolderParts.Length - equalParts;
        var replaceOld = string.Join(separator, oldFolderParts[..replaceOldCount]) + separator;
        var replaceNewCount = newFolderParts.Length - equalParts;
        var replaceNew = string.Join(separator, newFolderParts[..replaceNewCount]) + separator;

        // Apply replace to all paths.
        Apply(replaceOld, replaceNew);
    }
    
    /// <summary>
    /// Apply replace to all paths in the application.
    /// </summary>
    /// <param name="replaceOld">The old path to replace.</param>
    /// <param name="replaceNew">The new path to replace it with.</param>
    protected abstract void Apply(string replaceOld, string replaceNew);
    
    /// <summary>
    /// Call from within <see cref="Apply"/> to apply a replace to a specific path.
    /// </summary>
    /// <param name="filePath">The path to examine.</param>
    /// <param name="replaceOld">The old path to replace.</param>
    /// <param name="replaceNew">The new path to replace it with.</param>
    /// <returns>The new path after doing the replace.</returns>
    protected string Replace(string filePath, string replaceOld, string replaceNew) =>
        filePath.StartsWith(replaceOld) ? replaceNew + filePath[replaceOld.Length..] : filePath;
}
