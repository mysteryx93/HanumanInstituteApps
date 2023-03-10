using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

public class MainViewModel : MainViewModelBase<AppSettingsData>
{
    private readonly IAppPathService _appPath;
    private readonly IFileSystemService _fileSystem;
    private readonly IDialogService _dialogService;
    private readonly IPathFixer _pathFixer;

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    public MainViewModel(ISettingsProvider<AppSettingsData> settings, IAppUpdateService appUpdateService, IAppPathService appPath, 
        IFileSystemService fileSystem, IDialogService dialogService, IPathFixer pathFixer) :
        base(settings, appUpdateService)
    {
        _appPath = appPath;
        _fileSystem = fileSystem;
        _dialogService = dialogService;
        _pathFixer = pathFixer;

        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe((_) => ReloadFiles());
    }

    public override async void OnLoaded()
    {
        base.OnLoaded();
        await PromptFixPathsAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void ApplySettings()
    {
        this.RaisePropertyChanged(nameof(Settings));
        this.RaisePropertyChanged(nameof(EffectsFloat));
        this.RaisePropertyChanged(nameof(EffectsQuickAlgo));
        this.RaisePropertyChanged(nameof(EffectsSampleRateConversion));
    }

    [Reactive]
    public string SearchText { get; set; } = string.Empty;

    [Reactive]
    public int MasterVolume { get; set; } = 100;

    [Reactive]
    public int SelectedFolderIndex { get; set; } = -1;

    [Reactive]
    public bool IsPaused { get; set; }

    /// <summary>
    /// Gets or sets the currently selected preset.
    /// </summary>
    [Reactive]
    public PresetItem Playlist { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of audio files within the folders.
    /// </summary>
    [Reactive]
    public ICollectionView<FileItem> Files { get; private set; } = new CollectionView<FileItem>();

    /// <summary>
    /// Gets whether to use speed-shift quick algorithm based on settings.
    /// </summary>
    public bool EffectsFloat => Settings.PerformanceQuality == 5;

    /// <summary>
    /// Gets whether to use speed-shift quick algorithm based on settings.
    /// </summary>
    public bool EffectsQuickAlgo => Settings.PerformanceQuality <= 2;

    /// <summary>
    /// Gets the speed-shift sample rate conversion to use based on settings.
    /// </summary>
    public int EffectsSampleRateConversion => Math.Max(4, Settings.PerformanceQuality);

    /// <summary>
    /// Prompts to fix invalid paths, if any.
    /// </summary>
    public async Task PromptFixPathsAsync()
    {
        var changed = await _pathFixer.ScanAndFixFoldersAsync(this, Settings.Folders).ConfigureAwait(false);
        if (changed)
        {
            ReloadFiles();
            _settings.Save();
        }
    }

    /// <summary>
    /// Loads the list of files contained in Folders.
    /// </summary>
    public void ReloadFiles()
    {
        // If a folder is a sub-folder of another folder, remove it.
        var folders = Settings.Folders.ToList();
        var foldersSorted = Settings.Folders.OrderBy(x => x).ToList();
        foreach (var item in foldersSorted)
        {
            var matchList = foldersSorted.Where(x => x.StartsWith(item + _fileSystem.Path.DirectorySeparatorChar));
            folders.RemoveMany(matchList);
        }

        // Get the list of files.
        var files = folders.SelectMany(GetAudioFiles);

        // Apply search query.
        var query = SearchText.HasValue() ?
            files.Where(f => f.Display.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) != -1) :
            files;

        // Fill files list.
        query = query.OrderBy(f => f);
        Files.Source.Clear();
        Files.Source.AddRange(query);
    }

    /// <summary>
    /// Returns a list of all audio files in specified directory, searching recursively. Exceptions will be ignored and return an empty list.
    /// </summary>
    /// <param name="path">The path to search for audio files.</param>
    /// <returns>A list of audio files.</returns>
    private IEnumerable<FileItem> GetAudioFiles(string path)
    {
        path = path.EndsWith(_fileSystem.Path.DirectorySeparatorChar) ? path : path + _fileSystem.Path.DirectorySeparatorChar;
        return _fileSystem.GetFilesByExtensions(path, _appPath.AudioExtensions, System.IO.SearchOption.AllDirectories)
            .Select(x => new FileItem(x, TrimFolder(x, path)));

        string TrimFolder(string fullPath, string folder) =>
            fullPath.StartsWith(folder) ? fullPath[folder.Length..] : fullPath;
    }

    /// <summary>
    /// Stops playback of specified playback item and removes it from the list. 
    /// </summary>
    public ReactiveCommand<PlayingItem, Unit> RemoveMedia => _removeMedia ??= ReactiveCommand.Create<PlayingItem>(RemoveMediaImpl);
    private ReactiveCommand<PlayingItem, Unit>? _removeMedia;
    private void RemoveMediaImpl(PlayingItem? item)
    {
        if (item == null) { return; }

        Playlist.Files.Remove(item);
        if (!Playlist.Files.Any())
        {
            IsPaused = false;
        }
    }

    /// <summary>
    /// Adds a folder to the list of sources.
    /// </summary>
    public RxCommandUnit AddFolder => _addFolder ??= ReactiveCommand.CreateFromTask(AddFolderImpl);
    private RxCommandUnit? _addFolder;
    private async Task AddFolderImpl()
    {
        var result = await _dialogService.ShowOpenFolderDialogAsync(this).ConfigureAwait(true);
        if (result != null)
        {
            var newFolder = _fileSystem.Path.TrimEndingDirectorySeparator(result.LocalPath);
            if (!Settings.Folders.Contains(newFolder))
            {
                Settings.Folders.Add(newFolder);
            }
            SelectedFolderIndex = Settings.Folders.Count - 1;
            ReloadFiles();
        }
    }
    
    /// <summary>
    /// Removes selected folder from the list of sources. 
    /// </summary>
    public RxCommandUnit RemoveFolder => _removeFolder ??= ReactiveCommand.Create(RemoveFolderImpl,
        this.WhenAnyValue(x => x.SelectedFolderIndex).Select(x => x > -1));
    private RxCommandUnit? _removeFolder;
    private void RemoveFolderImpl()
    {
        if (SelectedFolderIndex > -1)
        {
            Settings.Folders.RemoveAt(SelectedFolderIndex);
            SelectedFolderIndex = Math.Min(SelectedFolderIndex, Settings.Folders.Count - 1);
            ReloadFiles();
        }
    }

    public void OnFilesListDoubleTap() => PlayImpl();
    public RxCommandUnit Play => _play ??= ReactiveCommand.Create(PlayImpl,
        this.WhenAnyValue(x => x.Files.CurrentPosition).Select(x => x > -1));
    private RxCommandUnit? _play;
    private void PlayImpl()
    {
        if (Files.CurrentItem != null)
        {
            // First first unused speed.
            var usedSpeeds = Playlist.Files.Where(x => x.FullPath == Files.CurrentItem.FullPath).Select(x => x.Speed).ToList();
            var speed = 0;
            var containCount = 1;
            while (usedSpeeds.Count(x => x == speed) >= containCount)
            {
                speed = speed > 0 ? -speed : -speed + 1;
                if (speed >= 5)
                {
                    speed = 0;
                    containCount++;
                }
            }

            // Play file with auto-calculated speed.
            Playlist.Files.Add(new PlayingItem(Files.CurrentItem.FullPath, Playlist.MasterVolume) { Speed = speed });
        }
    }
    
    /// <summary>
    /// Clears all playing audios.
    /// </summary>
    public RxCommandUnit Clear => _clear ??= ReactiveCommand.Create(ClearImpl,
        Playlist.Files.ToObservableChangeSet().ToCollection().Select(x => x.Any()));
    private RxCommandUnit? _clear;
    private void ClearImpl()
    {
        Playlist.Files.Clear();
    } 
    
    /// <summary>
    /// Shows the Load Preset dialog.
    /// </summary>
    public RxCommandUnit LoadPreset => _loadPreset ??= ReactiveCommand.CreateFromTask(LoadPresetImpl,
        Settings.Presets.ToObservableChangeSet().Select(x => x.Any()));
    private RxCommandUnit? _loadPreset;
    private async Task LoadPresetImpl()
    {
        var loadItem = await _dialogService.ShowLoadPresetViewAsync(this).ConfigureAwait(true);
        if (loadItem != null)
        {
            loadItem.SaveAs(Playlist);
            Playlist.MasterVolume = -1;
            Playlist.MasterVolume = loadItem.MasterVolume;
        }
    }

    /// <summary>
    /// Shows the Save Preset dialog.
    /// </summary>
    public RxCommandUnit SavePreset => _savePreset ??= ReactiveCommand.CreateFromTask(SavePresetImpl,
        Playlist.Files.ToObservableChangeSet().ToCollection().Select(x => x.Any()));
    private RxCommandUnit? _savePreset;
    private async Task SavePresetImpl()
    {
        var saveName = await _dialogService.ShowSavePresetViewAsync(this).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(saveName))
        {
            var preset = GetPresetByName(saveName);
            if (preset == null)
            {
                preset = new PresetItem();
                Settings.Presets.Add(preset);
            }

            Playlist.SaveAs(preset);
            preset.Name = saveName;
            _settings.Save();
        }
    }

    private PresetItem? GetPresetByName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return Settings.Presets.FirstOrDefault(p => p.Name == name);
        }

        return null;
    }
    
    /// <summary>
    /// Toggles the Play/Pause status of all playing audios.
    /// </summary>
    public RxCommandUnit Pause => _pause ??= ReactiveCommand.Create(PauseImpl);
    private RxCommandUnit? _pause;
    private void PauseImpl()
    {
        foreach (var item in Playlist.Files)
        {
            item.IsPlaying = IsPaused;
        }

        IsPaused = !IsPaused;
    }

    /// <inheritdoc />
    protected override Task ShowAboutImplAsync() => _dialogService.ShowAboutAsync(this);

    /// <inheritdoc />
    protected override Task ShowSettingsImplAsync() => _dialogService.ShowSettingsAsync(this, _settings.Value);
}
