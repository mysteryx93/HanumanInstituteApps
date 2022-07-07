using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Input;
using HanumanInstitute.MvvmDialogs;
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
    private readonly IDialogService _dialogService;

    public AppSettingsData AppData => _settings.Value;

    public MainViewModel(IPlaylistViewModelFactory playlistFactory, ISettingsProvider<AppSettingsData> settings,
        IFilesListViewModel filesListViewModel, IDialogService dialogService)
    {
        _playlistFactory = playlistFactory;
        _playlistFactory.OwnerViewModel = this;
        _settings = settings.CheckNotNull(nameof(settings));
        _filesListViewModel = filesListViewModel;
        _dialogService = dialogService;

        _settings.Loaded += Settings_Loaded;
        Settings_Loaded(_settings, EventArgs.Empty);

        Playlists.WhenAnyValue(x => x.CurrentItem).Subscribe((_) => Playlists_CurrentChanged());
    }
    
    public ICommand InitWindow => _initWindow ??= ReactiveCommand.CreateFromTask(InitWindowImplAsync);
    private ICommand? _initWindow;
    private async Task InitWindowImplAsync()
    {
        if (_settings.Value.ShowInfoOnStartup)
        {
            await Task.Delay(1).ConfigureAwait(true);
            await ShowAboutImplAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets or sets the height of the main window.
    /// </summary>
    public double WindowHeight
    {
        get => _settings.Value.Height;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._height, value);
    }

    /// <summary>
    /// Gets or sets the width of the main window.
    /// </summary>
    public double WindowWidth
    {
        get => _settings.Value.Width;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._width, value);
    }

    public PixelPoint WindowPosition
    {
        get => _settings.Value.Position;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._position, value);
    }

    public int Volume
    {
        get => _settings.Value.Volume;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._volume, value);
    }

    /// <summary>
    /// Returns the list of playlists with selection properties that can be bound to the UI.
    /// </summary>
    public ICollectionView<IPlaylistViewModel> Playlists { get; private set; } =
        new CollectionView<IPlaylistViewModel>();

    public ICommand StartPlayList => _startPlayList ??= ReactiveCommand.Create<TappedEventArgs>(StartPlayListImpl);
    private ICommand? _startPlayList;
    private void StartPlayListImpl(TappedEventArgs e)
    {
        _filesListViewModel.Files.CurrentItem = null;
        _filesListViewModel.PlayCommand.Execute(null);
    }

    public ICommand StartPlayFile => _startPlayFile ??= ReactiveCommand.Create<TappedEventArgs>(StartPlayFileImpl);
    private ICommand? _startPlayFile;
    private void StartPlayFileImpl(TappedEventArgs e) => _filesListViewModel.PlayCommand.Execute(null);

    /// <summary>
    /// Adds a new playlist to the list.
    /// </summary>
    public ICommand AddPlaylist => _addPlaylist ??= ReactiveCommand.Create(AddPlaylistImpl);
    private ICommand? _addPlaylist;
    private void AddPlaylistImpl()
    {
        var newPlaylist = _playlistFactory.Create();
        Playlists.Source.Add(newPlaylist);
        Playlists.MoveCurrentToLast();
    }

    /// <summary>
    /// Deletes selected playlist from the list.
    /// </summary>
    public ICommand DeletePlaylist => _deletePlaylist ??= ReactiveCommand.Create(DeletePlaylistImpl,
        this.WhenAnyValue(x => x.Playlists.CurrentItem).Select(x => x != null));
    private ICommand? _deletePlaylist;
    private void DeletePlaylistImpl()
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
        Playlists.Source.AddRange(playlists.Select(x => _playlistFactory.Create(x)));
    }

    /// <summary>
    /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
    /// </summary>
    public ICommand SaveSettings => _saveSettings ??= ReactiveCommand.Create<CancelEventArgs>(SaveSettingsImpl);
    private ICommand? _saveSettings;
    private void SaveSettingsImpl(CancelEventArgs e)
    {
        _settings.Value.Playlists.Clear();
        _settings.Value.Playlists.AddRange(Playlists.Source.Select(x =>
            new SettingsPlaylistItem(x.Name, x.Folders.Source)));
        _settings.Save();
    }

    /// <summary>
    /// Shows the About window.
    /// </summary>
    public ICommand ShowAbout => _showAbout ??= ReactiveCommand.CreateFromTask(ShowAboutImplAsync);
    private ICommand? _showAbout;
    private async Task ShowAboutImplAsync()
    {
        var vm = _dialogService.CreateViewModel<AboutViewModel>();
        await _dialogService.ShowDialogAsync(this, vm).ConfigureAwait(false);
        _settings.Save();
    }
    
    // /// <summary>
    // /// Shows the Settings window.
    // /// </summary>
    // public ICommand ShowSettings => _showSettings ??= ReactiveCommand.CreateFromTask(ShowSettingsImplAsync);
    // private ICommand? _showSettings;
    // private Task ShowSettingsImplAsync()
    // {
    //
    // }
}
