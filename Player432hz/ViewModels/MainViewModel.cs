using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using HanumanInstitute.Common.Avalonia;
using HanumanInstitute.Common.Services;
using HanumanInstitute.Player432hz.Models;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <summary>
/// Represents the playlist editor.
/// </summary>
public class MainViewModel : ReactiveObject
{
    private readonly IPlaylistViewModelFactory _playlistFactory;
    private readonly ISettingsProvider<AppSettingsData> _settings;
    private readonly IFilesListViewModel _filesListViewModel;

    public AppSettingsData AppData => _settings.Value;

    public MainViewModel(IPlaylistViewModelFactory playlistFactory, ISettingsProvider<AppSettingsData> settings,
        IFilesListViewModel filesListViewModel)
    {
        _playlistFactory = playlistFactory;
        _settings = settings.CheckNotNull(nameof(settings));
        _filesListViewModel = filesListViewModel;

        _settings.Loaded += Settings_Loaded;
        Settings_Loaded(_settings, EventArgs.Empty);

        Playlists.WhenAnyValue(x => x.CurrentItem).Subscribe((_) => Playlists_CurrentChanged());
    }

    /// <summary>
    /// Gets or sets the height of the main window.
    /// </summary>
    public double WindowHeight
    {
        get => _settings.Value.Height;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._height, value, nameof(WindowHeight));
    }

    /// <summary>
    /// Gets or sets the width of the main window.
    /// </summary>
    public double WindowWidth
    {
        get => _settings.Value.Width;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._width, value, nameof(WindowWidth));
    }

    public PixelPoint WindowPosition
    {
        get => _settings.Value.Position;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._position, value, nameof(WindowPosition));
    }

    public int Volume
    {
        get => _settings.Value.Volume;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._volume, value, nameof(Volume));
    }

    /// <summary>
    /// Returns the list of playlists with selection properties that can be bound to the UI.
    /// </summary>
    public ICollectionView<IPlaylistViewModel> Playlists { get; private set; } =
        new CollectionView<IPlaylistViewModel>();

    public ICommand PlayListCommand => _playListCommand ??= ReactiveCommand.Create<TappedEventArgs>(OnPlayList);
    private ICommand? _playListCommand;
    private void OnPlayList(TappedEventArgs e)
    {
        _filesListViewModel.Files.CurrentItem = null;
        _filesListViewModel.PlayCommand.Execute(null);
    }

    public ICommand PlayFileCommand => _playFileCommand ??= ReactiveCommand.Create<TappedEventArgs>(OnPlayFile);
    private ICommand? _playFileCommand;
    private void OnPlayFile(TappedEventArgs e) => _filesListViewModel.PlayCommand.Execute(null);

    /// <summary>
    /// Adds a new playlist to the list.
    /// </summary>
    public ICommand AddPlaylistCommand => _addPlaylistCommand ??= ReactiveCommand.Create(OnAddPlaylist);
    private ICommand? _addPlaylistCommand;
    private void OnAddPlaylist()
    {
        var newPlaylist = _playlistFactory.Create();
        Playlists.Source.Add(newPlaylist);
        Playlists.MoveCurrentToLast();
    }

    /// <summary>
    /// Deletes selected playlist from the list.
    /// </summary>
    public ICommand DeletePlaylistCommand => _deletePlaylistCommand ??= ReactiveCommand.Create(OnDeletePlaylist,
        this.WhenAnyValue(x => x.Playlists.CurrentItem).Select(x => x != null));
    private ICommand? _deletePlaylistCommand;
    private void OnDeletePlaylist()
    {
        if (Playlists.CurrentPosition > -1)
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
    private void Playlists_CurrentChanged()
    {
        _filesListViewModel.SetPaths(Playlists.CurrentItem?.Folders?.Source);
    }

    /// <summary>
    /// After settings are loaded, get the list of playlists converted into PlaylistViewModel.
    /// </summary>
    private void Settings_Loaded(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(WindowHeight));
        this.RaisePropertyChanged(nameof(WindowWidth));

        var playlists = _settings.Value.Playlists;
        Playlists.Source.Clear();
        foreach (var item in playlists.Select(x => _playlistFactory.Create(x)))
        {
            Playlists.Source.Add(item);
        }
    }

    /// <summary>
    /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
    /// </summary>
    public ICommand SaveSettingsCommand => _saveSettingsCommand ??= ReactiveCommand.Create<CancelEventArgs>(OnSaveSettings);
    private ICommand? _saveSettingsCommand;
    private void OnSaveSettings(CancelEventArgs e)
    {
        _settings.Value.Playlists.Clear();
        _settings.Value.Playlists.AddRange(Playlists.Source.Select(x =>
            new SettingsPlaylistItem(x.Name, x.Folders.Source)));
        _settings.Save();
    }
}
