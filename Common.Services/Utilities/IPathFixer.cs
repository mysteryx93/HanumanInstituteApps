using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Common.Services;

public interface IPathFixer
{
    /// <summary>
    /// Scans specified list of folders to detect any missing folder.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to <see cref="IDialogService"/>.</param>
    /// <param name="folders">A list of folders to examine. Do not include file names.</param>
    Task<bool> ScanAndFixFoldersAsync<T>(INotifyPropertyChanged owner, IList<T> folders)
        where T : class;
    
    /// <summary>
    /// Scans specified list of folders to detect any missing folder.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to <see cref="IDialogService"/>.</param>
    /// <param name="folders">A list of folders to examine. Do not include file names.</param>
    /// <param name="selector">If the list of folders are not string, converts a folder item into its path string.</param>
    Task<bool> ScanAndFixFoldersAsync<T>(INotifyPropertyChanged owner, IList<T> folders, Func<T, string?>? selector)
        where T : class;
    
    /// <summary>
    /// Scans specified list of folders to detect any missing folder. This overload allows specifying multiple lists.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to <see cref="IDialogService"/>.</param>
    /// <param name="folders">A list of lists of folders to examine. Do not include file names.</param>
    Task<bool> ScanAndFixFoldersMultipleAsync<T>(INotifyPropertyChanged owner, IList<IList<T>> folders)
        where T : class;

    /// <summary>
    /// Scans specified list of folders to detect any missing folder. This overload allows specifying multiple lists.
    /// If a broken path is found, it will prompt the user to locate the folder, and auto-fix application paths.
    /// </summary>
    /// <param name="owner">The ViewModel that will own dialog interactions. It will be passed to <see cref="IDialogService"/>.</param>
    /// <param name="folders">A list of lists of folders to examine. Do not include file names.</param>
    /// <param name="selector">If the list of folders are not string, converts a folder item into its path string.</param>
    Task<bool> ScanAndFixFoldersMultipleAsync<T>(INotifyPropertyChanged owner, IList<IList<T>> folders, Func<T, string?>? selector)
        where T : class;
}
