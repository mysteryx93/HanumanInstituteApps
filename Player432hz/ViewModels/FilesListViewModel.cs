using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Player432hz.Business;

namespace HanumanInstitute.Player432hz.ViewModels
{
    /// <summary>
    /// Represents a list of files to view the content of specified folders.
    /// </summary>
    public class FilesListViewModel : ViewModelBase, IFilesListViewModel
    {
        private readonly IFileLocator _fileLocator;
        private readonly IPlaylistPlayer _playlistPlayer;

        public FilesListViewModel(IFileLocator fileLocator, IPlaylistPlayer playlistPlayer)
        {
            _fileLocator = fileLocator;
            _playlistPlayer = playlistPlayer;
        }

        /// <summary>
        /// Gets the list of files and selection properties.
        /// </summary>
        public SelectableList<string> Files
        {
            get
            {
                if (!_loaded)
                {
                    Load();
                    _loaded = true;
                }
                return _files;
            }
        }
        private readonly SelectableList<string> _files = new SelectableList<string>();

        /// <summary>
        /// Sets the folder paths from which to load files.
        /// </summary>
        /// <param name="paths">The list of folder paths to load.</param>
        public void SetPaths(IEnumerable<string>? paths)
        {
            _paths = paths?.ToList();
            _files.List.Clear();
            _loaded = false;
        }
        private List<string>? _paths;
        private bool _loaded;

        /// <summary>
        /// Loads the list of audio files contained within the list of folders.
        /// </summary>
        private void Load()
        {
            _files.List.Clear();
            if (_paths != null)
            {
                foreach (var item in _fileLocator.GetAudioFiles(_paths))
                {
                    _files.List.Add(item);
                }
            }
        }

        /// <summary>
        /// Starts playing the selected playlist. If string parameter is specified, the specified file path will be played first.
        /// </summary>
        public ICommand PlayCommand => CommandHelper.InitCommand(ref _playCommand, OnPlay, CanPlay);
        private RelayCommand? _playCommand;
        private bool CanPlay() => Files?.List?.Any() == true;
        private void OnPlay()
        {
            if (CanPlay())
            {
                _playlistPlayer.Play(Files.List, Files.SelectedItem);
            }
        }
    }
}
