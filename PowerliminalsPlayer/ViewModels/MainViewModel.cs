using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.Common.Avalonia;
using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Models;
using HanumanInstitute.PowerliminalsPlayer.Business;
using MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IAppPathService _appPath;
    private readonly ISettingsProvider<AppSettingsData> _settings;
    private readonly IFileSystemService _fileSystem;
    private readonly IDialogService _dialogService;
    private readonly IFolderPathFixer _folderPathFixer;

    public AppSettingsData AppData => _settings.Value;

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    public MainViewModel(IAppPathService appPath, ISettingsProvider<AppSettingsData> appSettings,
        IFileSystemService fileSystem, IDialogService dialogService, IFolderPathFixer folderPathFixer)
    {
        _appPath = appPath;
        _settings = appSettings;
        _fileSystem = fileSystem;
        _dialogService = dialogService;
        _folderPathFixer = folderPathFixer;

        this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(200)).Subscribe((_) => ReloadFiles());
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }
    private string _searchText = string.Empty;

    public int MasterVolume
    {
        get => _masterVolume;
        set => this.RaiseAndSetIfChanged(ref _masterVolume, value);
    }
    private int _masterVolume = 100;

    public int SelectedFolderIndex
    {
        get => _selectedFolderIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedFolderIndex, value);
    }
    private int _selectedFolderIndex = -1;

    public bool IsPaused
    {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }
    private bool _isPaused;

    /// <summary>
    /// Gets or sets the currently selected preset.
    /// </summary>
    public PresetItem Playlist
    {
        get => _playlist;
        set => this.RaiseAndSetIfChanged(ref _playlist, value);
    }
    private PresetItem _playlist = new();

    /// <summary>
    /// Gets or sets the list of files currently playing.
    /// </summary>
    public ICollectionView<string> Files { get; private set; } = new CollectionView<string>();

    /// <summary>
    /// Loads the settings file.
    /// </summary>
    // public ICommand LoadSettingsCommand => _loadSettingsCommand ??= ReactiveCommand.Create(LoadSettings);
    // private ICommand? _loadSettingsCommand;
    public void LoadSettings()
    {
        _settings.Load();
        this.RaisePropertyChanged(nameof(AppData));
        ReloadFiles();
    }

    /// <summary>
    /// Saves the settings file.
    /// </summary>
    public ICommand SaveSettingsCommand => _saveSettingsCommand ??= ReactiveCommand.Create(SaveSettings);
    private ICommand? _saveSettingsCommand;
    public void SaveSettings() => _settings.Save();

    public Task PromptFixPathsAsync() => _folderPathFixer.PromptFixPathsAsync(this);

    /// <summary>
    /// Loads the list of files contained in Folders.
    /// </summary>
    private void ReloadFiles()
    {
        // If a folder is a sub-folder of another folder, remove it.
        var folders = AppData.Folders.ToList();
        var foldersSorted = AppData.Folders.OrderBy(x => x).ToList();
        foreach (var item in foldersSorted)
        {
            var matchList = foldersSorted.Where(x => x.StartsWith(item + _fileSystem.Path.DirectorySeparatorChar));
            folders.RemoveMany(matchList);
        }

        // Get the list of files.
        var files = folders.SelectMany(GetAudioFiles);

        // Apply search query.
        var query = string.IsNullOrEmpty(SearchText)
            ? files
            : files.Where(f => f.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) != -1);

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
    private IEnumerable<string> GetAudioFiles(string path)
    {
        return _fileSystem.GetFilesByExtensions(path, _appPath.AudioExtensions, System.IO.SearchOption.AllDirectories);
    }

    public ICommand RemoveMediaCommand => _removeMediaCommand ??= ReactiveCommand.Create<FileItem>(OnRemoveMedia);
    private ICommand? _removeMediaCommand;
    public void OnRemoveMedia(FileItem? item)
    {
        if (item == null) { return; }

        Playlist.Files.Remove(item);
        if (!Playlist.Files.Any())
        {
            IsPaused = false;
        }
    }

    public ICommand AddFolderCommand => _addFolderCommand ??= ReactiveCommand.CreateFromTask(OnAddFolder);
    private ICommand? _addFolderCommand;
    private async Task OnAddFolder()
    {
        var result = await _dialogService.ShowOpenFolderDialogAsync(this).ConfigureAwait(true);
        if (result.HasValue())
        {
            var newFolder = _fileSystem.Path.TrimEndingDirectorySeparator(result);
            if (!AppData.Folders.Contains(newFolder))
            {
                AppData.Folders.Add(newFolder);
            }
            SelectedFolderIndex = AppData.Folders.Count - 1;
            ReloadFiles();
        }
    }

    public ICommand RemoveFolderCommand => _removeFolderCommand ??= ReactiveCommand.Create(OnRemoveFolder,
        this.WhenAnyValue(x => x.SelectedFolderIndex).Select(x => x > -1));
    private ICommand? _removeFolderCommand;
    private void OnRemoveFolder()
    {
        if (SelectedFolderIndex > -1)
        {
            AppData.Folders.RemoveAt(SelectedFolderIndex);
            SelectedFolderIndex = Math.Min(SelectedFolderIndex, AppData.Folders.Count - 1);
            ReloadFiles();
        }
    }

    public void OnFilesListDoubleTap() => OnPlay();

    public ICommand PlayCommand => _playCommand ??= ReactiveCommand.Create(OnPlay,
        this.WhenAnyValue(x => x.Files.CurrentPosition).Select(x => x > -1));
    private ICommand? _playCommand;
    private void OnPlay()
    {
        if (Files.CurrentItem != null)
        {
            Playlist.Files.Add(new FileItem(Files.CurrentItem, Playlist.MasterVolume));
        }
    }

    public ICommand LoadPresetCommand => _loadPresetCommand ??= ReactiveCommand.Create(OnLoadPreset,
        AppData.Presets.ToObservableChangeSet().Select(x => x.Any()));
    private ICommand? _loadPresetCommand;
    private async void OnLoadPreset()
    {
        var loadItem = await _dialogService.ShowLoadPresetViewAsync(this).ConfigureAwait(true);
        if (loadItem != null)
        {
            loadItem.SaveAs(Playlist);
            Playlist.MasterVolume = -1;
            Playlist.MasterVolume = loadItem.MasterVolume;
        }
    }

    public ICommand SavePresetCommand => _savePresetCommand ??= ReactiveCommand.Create(OnSavePreset,
        Playlist.Files.ToObservableChangeSet().Select(x => x.Any()));
    private ICommand? _savePresetCommand;
    private async void OnSavePreset()
    {
        var saveName = await _dialogService.ShowSavePresetViewAsync(this).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(saveName))
        {
            var preset = GetPresetByName(saveName);
            if (preset == null)
            {
                preset = new PresetItem();
                AppData.Presets.Add(preset);
            }

            Playlist.SaveAs(preset);
            preset.Name = saveName;
            SaveSettings();
        }
    }

    private PresetItem? GetPresetByName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return AppData.Presets.FirstOrDefault(p => p.Name == name);
        }

        return null;
    }

    public ICommand PauseCommand => _pauseCommand ??= ReactiveCommand.Create(OnPause);
    private ICommand? _pauseCommand;
    private void OnPause()
    {
        foreach (var item in Playlist.Files)
        {
            item.IsPlaying = IsPaused;
        }

        IsPaused = !IsPaused;
    }
}
