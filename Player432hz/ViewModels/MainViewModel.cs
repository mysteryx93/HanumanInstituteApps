using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.CommonWpfApp;
using GalaSoft.MvvmLight.CommandWpf;

namespace HanumanInstitute.Player432hz.ViewModels
{
    /// <summary>
    /// Represents the playlist editor.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IPlaylistViewModelFactory _playlistFactory;
        private readonly ISettingsProvider _settings;
        private readonly IFilesListViewModel _filesListViewModel;

        public MainViewModel(IPlaylistViewModelFactory playlistFactory, ISettingsProvider settings, IFilesListViewModel filesListViewModel)
        {
            _playlistFactory = playlistFactory;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _filesListViewModel = filesListViewModel;

            _settings.Loaded += Settings_Loaded;
            _settings.Saving += Settings_Saving;
            Settings_Loaded(null, null);

            Playlists.PropertyChanged += Playlists_PropertyChanged;
        }

        /// <summary>
        /// Returns the list of playlists with selection properties that can be bound to the UI.
        /// </summary>
        public ISelectableList<IPlaylistViewModel> Playlists { get; private set; } = new SelectableList<IPlaylistViewModel>();

        public ICommand PlayCommand => _filesListViewModel.PlayCommand;

        /// <summary>
        /// Adds a new playlist to the list.
        /// </summary>
        public ICommand AddPlaylistCommand => CommandHelper.InitCommand(ref addPlaylistCommand, OnAddPlaylist, () => CanAddPlaylist);
        private RelayCommand addPlaylistCommand;
        private bool CanAddPlaylist => Playlists.List != null;
        private void OnAddPlaylist()
        {
            if (CanAddPlaylist)
            {
                var NewPlaylist = _playlistFactory.Create();
                Playlists.List.Add(NewPlaylist);
                Playlists.SelectedIndex = Playlists.List.Count - 1;
            }
        }

        /// <summary>
        /// Deletes selected playlist from the list.
        /// </summary>
        public ICommand DeletePlaylistCommand => CommandHelper.InitCommand(ref deletePlaylistCommand, OnDeletePlaylist, () => CanDeletePlaylist);
        private RelayCommand deletePlaylistCommand;
        private bool CanDeletePlaylist => Playlists.HasSelection;
        private void OnDeletePlaylist()
        {
            if (CanDeletePlaylist)
            {
                Playlists.List.RemoveAt(Playlists.SelectedIndex);
                if (Playlists.SelectedIndex >= Playlists.List.Count)
                {
                    Playlists.SelectedIndex = Playlists.List.Count - 1;
                }
            }
        }

        /// <summary>
        /// When a playlist is selected, display the files.
        /// </summary>
        /// <param name="list"></param>
        private void Playlists_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Playlists.SelectedItem))
            {
                _filesListViewModel.SetPaths(Playlists?.SelectedItem?.Folders?.List);
            }
        }

        /// <summary>
        /// After settings are loaded, get the list of playlists converted into PlaylistViewModel.
        /// </summary>
        private void Settings_Loaded(object sender, EventArgs e)
        {
            Playlists.List.Clear();
            var playlists = _settings?.Current?.Playlists;
            if (playlists != null)
            {
                Playlists.ReplaceAll(playlists.Select(x => _playlistFactory.Create(x)));
            }
        }

        /// <summary>
        /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
        /// </summary>
        private void Settings_Saving(object sender, EventArgs e)
        {
            _settings.Current.Playlists.Clear();
            _settings.Current.Playlists.AddRange(Playlists.List.Select(x => new SettingsPlaylistItem(x.Name, x.Folders.List)));
        }
    }
}
