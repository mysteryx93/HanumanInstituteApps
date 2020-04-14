using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using HanumanInstitute.CommonUI;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Player432hz.Business;

namespace HanumanInstitute.Player432hz.ViewModels
{
    /// <summary>
    /// Represents a playlist for viewing and editing.
    /// </summary>
    public class PlaylistViewModel : ViewModelBase, IPlaylistViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IFilesListViewModel _fileListViewModel;

        public PlaylistViewModel(IDialogService dialogService, IFilesListViewModel fileListViewModel) :
            this(dialogService, fileListViewModel, null)
        { }

        public PlaylistViewModel(IDialogService dialogService, IFilesListViewModel fileListViewModel, SettingsPlaylistItem data)
        {
            _dialogService = dialogService;
            _fileListViewModel = fileListViewModel;

            if (data != null)
            {
                Name = data.Name;
                Folders.ReplaceAll(data.Folders);
            }
        }

        /// <summary>
        /// Gets or sets the name of this playlist.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the list of folders in the playlist and provides selection properties.
        /// </summary>
        public SelectableList<string> Folders { get; set; } = new SelectableList<string>();

        /// <summary>
        /// Shows a folder picker and adds selected folder to the list.
        /// </summary>
        public ICommand AddFolderCommand => CommandHelper.InitCommand(ref addFolderCommand, OnAddFolder, () => CanAddFolder);
        private ICommand addFolderCommand;
        private static bool CanAddFolder => true;
        private void OnAddFolder()
        {
            if (CanAddFolder)
            {
                var folderSettings = new FolderBrowserDialogSettings();
                _dialogService.ShowFolderBrowserDialog(this, folderSettings);
                if (!string.IsNullOrEmpty(folderSettings.SelectedPath))
                {
                    Folders.List.Add(folderSettings.SelectedPath);
                    Folders.Select(Folders.List.Count - 1);
                    _fileListViewModel.SetPaths(Folders.List);
                }
            }
        }

        /// <summary>
        /// Removes selected folder from the list.
        /// </summary>
        public ICommand RemoveFolderCommand => CommandHelper.InitCommand(ref removeFolderCommand, OnRemoveFolder, () => CanRemoveFolder);
        private ICommand removeFolderCommand;
        private bool CanRemoveFolder => Folders.SelectedIndex > -1 && Folders.SelectedIndex < Folders.List.Count;
        private void OnRemoveFolder()
        {
            if (CanRemoveFolder)
            {
                var selection = Folders.SelectedIndex;
                Folders.List.RemoveAt(Folders.SelectedIndex);
                Folders.Select(selection, true);
                _fileListViewModel.SetPaths(Folders.List);
            }
        }
    }
}
