using System.Collections.Generic;
using System.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Services;

public interface IPathFixer
{
    /// <summary>
    /// Scans specified list of folders to detect any missing folder.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to <see cref="IDialogService"/>.</param>
    /// <param name="folders">A list of folders to examine. Do not include file names.</param>
    Task<bool> ScanAndFixFoldersAsync(INotifyPropertyChanged owner, IList<string> folders);
    
    /// <summary>
    /// Scans specified list of folders to detect any missing folder.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to <see cref="IDialogService"/>.</param>
    /// <param name="folders">A list of folders to examine. Do not include file names.</param>
    Task<bool> ScanAndFixFoldersAsync(INotifyPropertyChanged owner, FixFolderItem folders);

    /// <summary>
    /// Scans specified list of folders to detect any missing folder. This overload allows specifying multiple lists.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to <see cref="IDialogService"/>.</param>
    /// <param name="folders">A list of lists of folders to examine. Do not include file names.</param>
    Task<bool> ScanAndFixFoldersAsync(INotifyPropertyChanged owner, IList<FixFolderItem> folders);
}
