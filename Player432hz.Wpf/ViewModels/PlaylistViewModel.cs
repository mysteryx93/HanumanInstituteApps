using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonAvalonia;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MediaPlayer.Avalonia.Helpers.Mvvm;
using HanumanInstitute.Player432hz.Business;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels
{
    /// <summary>
    /// Represents a playlist for viewing and editing.
    /// </summary>
    public class PlaylistViewModel : ReactiveObject, IPlaylistViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IFilesListViewModel _fileListViewModel;

        public PlaylistViewModel(IDialogService dialogService, IFilesListViewModel fileListViewModel) :
            this(dialogService, fileListViewModel, null)
        { }

        public PlaylistViewModel(IDialogService dialogService, IFilesListViewModel fileListViewModel, SettingsPlaylistItem? data)
        {
            _dialogService = dialogService;
            _fileListViewModel = fileListViewModel;

            if (data != null)
            {
                Name = data.Name;
                Folders.Source.Clear();
                foreach (var item in data.Folders)
                {
                    Folders.Source.Add(item);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of this playlist.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the list of folders in the playlist and provides selection properties.
        /// </summary>
        public ICollectionView<string> Folders { get; set; } = new CollectionView<string>();

        /// <summary>
        /// Shows a folder picker and adds selected folder to the list.
        /// </summary>
        public ICommand AddFolderCommand => _addFolderCommand ??= new RelayCommand(OnAddFolder, CanAddFolder);
        private RelayCommand? _addFolderCommand;
        private static bool CanAddFolder() => true;
        private void OnAddFolder()
        {
            if (CanAddFolder())
            {
                var folderSettings = new FolderBrowserDialogSettings();
                _dialogService.ShowFolderBrowserDialog(this, folderSettings);
                if (!string.IsNullOrEmpty(folderSettings.SelectedPath))
                {
                    Folders.Source.Add(folderSettings.SelectedPath);
                    Folders.MoveCurrentToLast();
                    _fileListViewModel.SetPaths(Folders.Source);
                }
            }
        }

        /// <summary>
        /// Removes selected folder from the list.
        /// </summary>
        public ICommand RemoveFolderCommand =>
            _removeFolderCommand ??= new RelayCommand(OnRemoveFolder, CanRemoveFolder);
        private RelayCommand? _removeFolderCommand;
        private bool CanRemoveFolder() => Folders.CurrentPosition > -1 && Folders.CurrentPosition < Folders.Source.Count;
        private void OnRemoveFolder()
        {
            if (CanRemoveFolder())
            {
                var selection = Folders.CurrentPosition;
                Folders.Source.RemoveAt(Folders.CurrentPosition);
                Folders.MoveCurrentToPosition(selection);
                _fileListViewModel.SetPaths(Folders.Source);
            }
        }
    }
}
