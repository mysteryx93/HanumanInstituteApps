using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
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

    public MainViewModel(IPlaylistViewModelFactory playlistFactory, ISettingsProvider<AppSettingsData> settings,
        IFilesListViewModel filesListViewModel, IDialogService dialogService)
    {
        _playlistFactory = playlistFactory;
        _playlistFactory.OwnerViewModel = this;
        _settings = settings.CheckNotNull(nameof(settings));
        _filesListViewModel = filesListViewModel;
        _dialogService = dialogService;

        _settings.Changed += Settings_Loaded;
        Settings_Loaded(_settings, EventArgs.Empty);

        Playlists.WhenAnyValue(x => x.CurrentItem).Subscribe((_) => Playlists_CurrentChanged());
    }

    public AppSettingsData AppSettings => _settings.Value;

    public RxCommandUnit InitWindow => _initWindow ??= ReactiveCommand.CreateFromTask(InitWindowImplAsync);
    private RxCommandUnit? _initWindow;
    private async Task InitWindowImplAsync()
    {
        if (_settings.Value.ShowInfoOnStartup)
        {
            await Task.Delay(1).ConfigureAwait(true);
            await ShowAboutImplAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns the list of playlists with selection properties that can be bound to the UI.
    /// </summary>
    public ICollectionView<IPlaylistViewModel> Playlists { get; private set; } =
        new CollectionView<IPlaylistViewModel>();

    public ReactiveCommand<TappedEventArgs, Unit> StartPlayList => _startPlayList ??= ReactiveCommand.Create<TappedEventArgs>(StartPlayListImpl);
    private ReactiveCommand<TappedEventArgs, Unit>? _startPlayList;
    private void StartPlayListImpl(TappedEventArgs e)
    {
        _filesListViewModel.Files.CurrentPosition = -1;
        _filesListViewModel.PlayCommand.Execute().Subscribe();
    }

    public ReactiveCommand<TappedEventArgs, Unit> StartPlayFile => _startPlayFile ??= ReactiveCommand.Create<TappedEventArgs>(StartPlayFileImpl);
    private ReactiveCommand<TappedEventArgs, Unit>? _startPlayFile;
    private void StartPlayFileImpl(TappedEventArgs e) => _filesListViewModel.PlayCommand.Execute().Subscribe();

    /// <summary>
    /// Adds a new playlist to the list.
    /// </summary>
    public RxCommandUnit AddPlaylist => _addPlaylist ??= ReactiveCommand.Create(AddPlaylistImpl);
    private RxCommandUnit? _addPlaylist;
    private void AddPlaylistImpl()
    {
        var newPlaylist = _playlistFactory.Create();
        Playlists.Source.Add(newPlaylist);
        Playlists.MoveCurrentToLast();
    }

    /// <summary>
    /// Deletes selected playlist from the list.
    /// </summary>
    public RxCommandUnit DeletePlaylist => _deletePlaylist ??= ReactiveCommand.Create(DeletePlaylistImpl,
        this.WhenAnyValue(x => x.Playlists.CurrentItem).Select(x => x != null));
    private RxCommandUnit? _deletePlaylist;
    private void DeletePlaylistImpl()
    {
        if (Playlists.CurrentPosition > -1)
        {
            Playlists.Source.RemoveAt(Playlists.CurrentPosition);
            // if (Playlists.CurrentPosition >= Playlists.Source.Count)
            // {
            //     Playlists.MoveCurrentToLast();
            // }
        }
    }

    /// <summary>
    /// When a playlist is selected, display the files.
    /// </summary>
    private void Playlists_CurrentChanged()
    {
        _filesListViewModel.SetPaths(Playlists.CurrentItem?.Folders.Source);
    }

    /// <summary>
    /// After settings are loaded, get the list of playlists converted into PlaylistViewModel.
    /// </summary>
    private void Settings_Loaded(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(AppSettings));
        var playlists = _settings.Value.Playlists;
        Playlists.Source.Clear();
        Playlists.Source.AddRange(playlists.Select(x => _playlistFactory.Create(x)));
    }

    /// <summary>
    /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
    /// </summary>
    public ReactiveCommand<CancelEventArgs, Unit> SaveSettings => _saveSettings ??= ReactiveCommand.Create<CancelEventArgs>(SaveSettingsImpl);
    private ReactiveCommand<CancelEventArgs, Unit>? _saveSettings;
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
    public RxCommandUnit ShowAbout => _showAbout ??= ReactiveCommand.CreateFromTask(ShowAboutImplAsync);
    private RxCommandUnit? _showAbout;
    private Task ShowAboutImplAsync() => _dialogService.ShowAboutAsync(this);

    /// <summary>
    /// Shows the Settings window.
    /// </summary>
    public RxCommandUnit ShowSettings => _showSettings ??= ReactiveCommand.CreateFromTask(ShowSettingsImplAsync);
    private RxCommandUnit? _showSettings;
    private Task ShowSettingsImplAsync() => _dialogService.ShowSettingsAsync(this, _settings.Value);
}
