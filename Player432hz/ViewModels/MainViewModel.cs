using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Player432hz.Business;

namespace HanumanInstitute.Player432hz.ViewModels
{
    /// <summary>
    /// Represents the playlist editor.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IPlaylistViewModelFactory _playlistFactory;
        private readonly ISettingsProvider<SettingsData> _settings;
        private readonly IFilesListViewModel _filesListViewModel;

        public MainViewModel(IPlaylistViewModelFactory playlistFactory, ISettingsProvider<SettingsData> settings, IFilesListViewModel filesListViewModel)
        {
            _playlistFactory = playlistFactory;
            _settings = settings.CheckNotNull(nameof(settings));
            _filesListViewModel = filesListViewModel;

            _settings.Loaded += Settings_Loaded;
            Settings_Loaded(_settings, new EventArgs());

            Playlists.CurrentChanged += Playlists_CurrentChanged;
        }

        /// <summary>
        /// Gets or sets the height of the main window.
        /// </summary>
        public double WindowHeight
        {
            get => _settings.Value.Height;
            set => _settings.Value.Height = value;
        }

        /// <summary>
        /// Gets or sets the width of the main window.
        /// </summary>
        public double WindowWidth
        {
            get => _settings.Value.Width;
            set => _settings.Value.Width = value;
        }

        public double WindowLeft
        {
            get => _settings.Value.Left;
            set => _settings.Value.Left = value;
        }

        public double WindowTop
        {
            get => _settings.Value.Top;
            set => _settings.Value.Top = value;
        }

        public int Volume
        {
            get => _settings.Value.Volume;
            set => _settings.Value.Volume = value;
        }

        /// <summary>
        /// Returns the list of playlists with selection properties that can be bound to the UI.
        /// </summary>
        public ICollectionView<IPlaylistViewModel> Playlists { get; private set; } = new CollectionView<IPlaylistViewModel>();

        public ICommand PlayCommand => _filesListViewModel.PlayCommand;

        /// <summary>
        /// Adds a new playlist to the list.
        /// </summary>
        public ICommand AddPlaylistCommand => CommandHelper.InitCommand(ref _addPlaylistCommand, OnAddPlaylist, () => CanAddPlaylist);
        private RelayCommand? _addPlaylistCommand;
        private bool CanAddPlaylist => Playlists.Source != null;
        private void OnAddPlaylist()
        {
            if (CanAddPlaylist)
            {
                var newPlaylist = _playlistFactory.Create();
                Playlists.Source.Add(newPlaylist);
                Playlists.MoveCurrentToLast();
            }
        }

        /// <summary>
        /// Deletes selected playlist from the list.
        /// </summary>
        public ICommand DeletePlaylistCommand => CommandHelper.InitCommand(ref _deletePlaylistCommand, OnDeletePlaylist, () => CanDeletePlaylist);
        private RelayCommand? _deletePlaylistCommand;
        private bool CanDeletePlaylist => Playlists.CurrentItem != null;
        private void OnDeletePlaylist()
        {
            if (CanDeletePlaylist)
            {
                Playlists.Source.RemoveAt(Playlists.CurrentPosition);
                if (Playlists.CurrentPosition >= Playlists.Source.Count)
                {
                    Playlists.MoveCurrentToLast();
                }
            }
        }

        /// <summary>
        /// When a playlist is selected, display the files.
        /// </summary>
        private void Playlists_CurrentChanged(object? sender, EventArgs e)
        {
            _filesListViewModel.SetPaths(Playlists?.CurrentItem?.Folders?.Source);
        }

        /// <summary>
        /// After settings are loaded, get the list of playlists converted into PlaylistViewModel.
        /// </summary>
        private void Settings_Loaded(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(WindowHeight));
            RaisePropertyChanged(nameof(WindowWidth));

            Playlists.Source.Clear();
            var playlists = _settings?.Value?.Playlists;
            if (playlists != null)
            {
                Playlists.Source.Clear();
                foreach (var item in playlists.Select(x => _playlistFactory.Create(x)))
                {
                    Playlists.Source.Add(item);
                }
            }
        }

        /// <summary>
        /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
        /// </summary>
        public void SaveSettings()
        {
            _settings.Value.Playlists.Clear();
            _settings.Value.Playlists.AddRange(Playlists.Source.Select(x => new SettingsPlaylistItem(x.Name, x.Folders.Source)));
            _settings.Save();
        }
    }
}
