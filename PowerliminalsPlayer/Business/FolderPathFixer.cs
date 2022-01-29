using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Models;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

public class FolderPathFixer : IFolderPathFixer
{
    private readonly IFileSystemService _fileSystem;
    private readonly IDialogService _dialogService;
    private readonly ISettingsProvider<AppSettingsData> _settings;

    public FolderPathFixer(IFileSystemService fileSystem, IDialogService dialogService, ISettingsProvider<AppSettingsData> settings)
    {
        _fileSystem = fileSystem;
        _dialogService = dialogService;
        _settings = settings;
    }

    public async Task PromptFixPathsAsync(INotifyPropertyChanged owner)
    {
        // var folders = _settings.Value.Folders;

        // Get the first folder not found.
        var invalidFolder = _settings.Value.Folders.FirstOrDefault(x => !_fileSystem.Directory.Exists(x));
        // string? invalidFolder = null;
        // string? folderName = null;
        // var invalidFolderPos = -1;
        // for (var i = 0; i < folders.Count; i++)
        // {
        //     var item = folders[i];
        //     if (!_fileSystem.Directory.Exists(item))
        //     {
        //         invalidFolder = _fileSystem.Path.TrimEndingDirectorySeparator(item);
        //         folderName = _fileSystem.Path.GetFileName(invalidFolder);
        //         invalidFolderPos = i;
        //         break;
        //     }
        // }

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
                await BrowseNewPathAsync(owner, invalidFolder, folderName);
            }
        }
    }

    private async Task BrowseNewPathAsync(INotifyPropertyChanged owner, string invalidFolder, string folderName)
    {
        // Browse for new path.
        var selectedPath = await _dialogService.ShowOpenFolderDialogAsync(owner).ConfigureAwait(true);
        if (selectedPath != null)
        {
            selectedPath = _fileSystem.Path.TrimEndingDirectorySeparator(selectedPath);
            var newPath = ProbeNewPath(selectedPath, folderName);
            if (newPath != null)
            {
                ApplyNewPathToAll(invalidFolder, newPath);
                await PromptFixPathsAsync(owner);
            }
            else
            {
                var message = $"Folder \"{folderName}\" not found at \"{newPath}\".\nWould you like to try again?";

                var result = await _dialogService.ShowMessageBoxAsync(owner,
                    message.Replace("\n", Environment.NewLine),
                    "Invalid Folder Path",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning).ConfigureAwait(true);

                if (result == true)
                {
                    await BrowseNewPathAsync(owner, invalidFolder, folderName).ConfigureAwait(true);
                }
            }
        }
    }

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

    private void ApplyNewPathToAll(string oldFolder, string newFolder)
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
        for (var i = 0; i < _settings.Value.Folders.Count; i++)
        {
            _settings.Value.Folders[i] = ReplaceStart(_settings.Value.Folders[i]);
        }
        foreach (var preset in _settings.Value.Presets)
        {
            foreach (var presetItem in preset.Files)
            {
                presetItem.FullPath = ReplaceStart(presetItem.FullPath);
            }
        }

        string ReplaceStart(string oldFile) =>
            oldFile.StartsWith(replaceOld) ? replaceNew + oldFile[replaceOld.Length..] : oldFile;
    }
}
