using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;

namespace HanumanInstitute.Converter432hz.ViewModels;

/// <summary>
/// Represents the main application window.
/// </summary>
public class MainViewModel : ReactiveObject
{
    private readonly ISettingsProvider<AppSettingsData> _settings;
    private readonly IDialogService _dialogService;
    private readonly IFileSystemService _fileSystem;
    private readonly IFileLocator _fileLocator;
    private readonly IAppPathService _appPath;
    private readonly IEnvironmentService _environment;
    private readonly IPitchDetector _pitchDetector;

    public AppSettingsData AppSettings => _settings.Value;

    public MainViewModel(ISettingsProvider<AppSettingsData> settings, IEncoderService encoder, IDialogService dialogService,
        IFileSystemService fileSystem, IFileLocator fileLocator, IAppPathService appPath, IEnvironmentService environment,
        IPitchDetector pitchDetector)
    {
        _settings = settings;
        Encoder = encoder;
        Encoder.Owner = this;
        _dialogService = dialogService;
        _fileSystem = fileSystem;
        _fileLocator = fileLocator;
        _appPath = appPath;
        _environment = environment;
        _pitchDetector = pitchDetector;
        _settings.Changed += Settings_Loaded;
        Settings_Loaded(_settings, EventArgs.Empty);

        Encoder.Settings.PitchTo = 432;
        Encoder.Settings.MaxThreads = Math.Min(64, _environment.ProcessorCount);
        FormatsList.SelectedValue = EncodeFormat.Mp3;
        BitrateList.SelectedValue = 0;
        SampleRateList.SelectedValue = 48000;
        FileExistsActionList.SelectedValue = FileExistsAction.Ask;
        SourcesSelectedIndex = -1;

        this.WhenAnyValue(x => x.Encoder.FileExistsAction)
            .BindTo(this, x => x.FileExistsActionList.SelectedValue);
        this.WhenAnyValue(x => x.FileExistsActionList.SelectedValue)
            .BindTo(this, x => x.Encoder.FileExistsAction);

        _isBitrateVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x != EncodeFormat.Flac && x != EncodeFormat.Wav)
            .ToProperty(this, x => x.IsBitrateVisible);
        _isSampleRateVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x != EncodeFormat.Opus)
            .ToProperty(this, x => x.IsSampleRateVisible);
        _isQualitySpeedVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x == EncodeFormat.Mp3 || x == EncodeFormat.Flac)
            .ToProperty(this, x => x.IsQualitySpeedVisible);

        // _completedString = this.WhenAnyValue(x => x.FilesLeft, x => x.CompletedCount,
        //     (left, completed) => left > 0 || completed > 0 ?
        //         $"({completed} / {completed + left})" : string.Empty)
        //     .ToProperty(this, x => x.CompletedString);

        // Encoder.FileCompleted += (_, _) =>
        // {
        //     CalcFilesLeft();
        //     CalcCompleted();
        // };
    }
    
    private void Settings_Loaded(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(AppSettings));
    }

    public RxCommandUnit InitWindow => _initWindow ??= ReactiveCommand.CreateFromTask(InitWindowImplAsync);
    private RxCommandUnit? _initWindow;
    private async Task InitWindowImplAsync()
    {
        if (_settings.Value.ShowInfoOnStartup)
        {
            await Task.Delay(1).ConfigureAwait(true); // work-around rendering bug in Avalonia v0.10.15
            await ShowAboutImplAsync().ConfigureAwait(false);
        }
    }

    // /// <summary>
    // /// Gets the quantity of files left.
    // /// </summary>
    // [Reactive]
    // public int FilesLeft { get; private set; }
    //
    // /// <summary>
    // /// Gets the quantity of files completed.
    // /// </summary>
    // [Reactive]
    // public int CompletedCount { get; private set; }
    //
    // /// <summary>
    // /// Returns a string with "(CompletedCount / CompletedCount+FilesLeft)".
    // /// </summary>
    // public string CompletedString => _completedString.Value;
    // private readonly ObservableAsPropertyHelper<string> _completedString;

    /// <summary>
    /// Gets whether Bitrate control should be visible.
    /// </summary>
    public bool IsBitrateVisible => _isBitrateVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isBitrateVisible;

    /// <summary>
    /// Gets whether SampleRate control should be visible.
    /// </summary>
    public bool IsSampleRateVisible => _isSampleRateVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isSampleRateVisible;

    /// <summary>
    /// Gets whether QualitySpeed slider should be visible.
    /// </summary>
    public bool IsQualitySpeedVisible => _isQualitySpeedVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isQualitySpeedVisible;

    /// <summary>
    /// The encoder service.
    /// </summary>
    public IEncoderService Encoder { get; }

    /// <summary>
    /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
    /// </summary>
    public ReactiveCommand<CancelEventArgs, Unit> SaveSettingsCommand => _saveSettingsCommand ??= ReactiveCommand.Create<CancelEventArgs>(OnSaveSettings);
    private ReactiveCommand<CancelEventArgs, Unit>? _saveSettingsCommand;
    private void OnSaveSettings(CancelEventArgs e) => _settings.Save();

    [Reactive] public int SourcesSelectedIndex { get; set; }

    public RxCommandUnit AddFiles => _addFiles ??= ReactiveCommand.CreateFromTask(AddFilesImpl);
    private RxCommandUnit? _addFiles;
    private async Task AddFilesImpl()
    {
        // throw new NotSupportedException();
        var settings = new OpenFileDialogSettings()
        {
            Title = "Select files to convert",
            Filters = new List<FileFilter>()
            {
                new FileFilter("Audio files", _appPath.AudioExtensions),
                new FileFilter("All files", "*")
            }
        };
        var files = await _dialogService.ShowOpenFilesDialogAsync(this, settings);
        var validFiles = files.Where(x => _fileSystem.File.Exists(x)).ToList();

        var items = validFiles.Select(x => new FileItem(x, _fileSystem.Path.GetFileName(x))).ToList();
        ListExtensions.AddRange(Encoder.Sources, items);
        //CalcFilesLeft();

        // Auto-detect pitch.
        await items.ForEachAsync(async x =>
        {
            try
            {
                x.Pitch = await _pitchDetector.GetPitchAsync(x.Path);
            }
            catch (Exception)
            {
                // ignored
            }
        }).ConfigureAwait(false);
    }

    public RxCommandUnit AddFolder => _addFolder ??= ReactiveCommand.CreateFromTask(AddFolderImpl);
    private RxCommandUnit? _addFolder;
    private async Task AddFolderImpl()
    {
        var settings = new OpenFolderDialogSettings() { Title = "Convert all audio files in folder" };
        var folder = await _dialogService.ShowOpenFolderDialogAsync(this, settings);
        if (folder != null)
        {
            var folderName = _fileSystem.Path.GetFileName(folder.TrimEnd(_environment.DirectorySeparatorChar));
            var folderItem = new FolderItem(folder, _environment.DirectorySeparatorChar + folderName);
            var files = _fileLocator.GetAudioFiles(folder);
            folderItem.Files.AddRange(files.Select(x => new FileItem(x.Path, _fileSystem.Path.Combine(folderName, x.RelativePath))));
            Encoder.Sources.Add(folderItem);
            //CalcFilesLeft();
        }
    }

    public RxCommandUnit RemoveFile => _removeFile ??= ReactiveCommand.Create(RemoveFileImpl, 
        this.WhenAnyValue(x => x.SourcesSelectedIndex, index => index > -1));
    private RxCommandUnit? _removeFile;
    private void RemoveFileImpl()
    {
        if (SourcesSelectedIndex > -1 && SourcesSelectedIndex < Encoder.Sources.Count)
        {
            var sel = SourcesSelectedIndex;
            Encoder.Sources.RemoveAt(sel);
            SourcesSelectedIndex = -1;
            SourcesSelectedIndex = sel >= Encoder.Sources.Count ? Encoder.Sources.Count - 1 : sel;
            //CalcFilesLeft();
        }
    }

    public RxCommandUnit BrowseDestination => _browseDestination ??= ReactiveCommand.CreateFromTask(BrowseDestinationImpl);
    private RxCommandUnit? _browseDestination;
    private async Task BrowseDestinationImpl()
    {
        var settings = new OpenFolderDialogSettings() { Title = "Destination" };
        var folder = await _dialogService.ShowOpenFolderDialogAsync(this, settings);
        if (folder != null)
        {
            Encoder.Destination = folder;
        }
    }

    // private void CalcFilesLeft() => FilesLeft =
    //     Encoder.Sources.Select(x => x switch
    //     {
    //         FolderItem folder => folder.Files.Count,
    //         _ => 1
    //     }).Sum(x => x);
    //
    // private void CalcCompleted() => CompletedCount =
    //     Encoder.ProcessingFiles.Count(x => x.Status == EncodeStatus.Completed);

    public ListItemCollectionView<EncodeFormat> FormatsList { get; } = new()
    {
        { EncodeFormat.Mp3, "MP3" },
        { EncodeFormat.Aac, "AAC" }, //AAC file encoder creates no valid containers and no tags
        { EncodeFormat.Wav, "WAV" },
        { EncodeFormat.Flac, "FLAC" },
        { EncodeFormat.Ogg, "OGG" },
        { EncodeFormat.Opus, "OPUS" }
    };

    public ListItemCollectionView<int> SampleRateList { get; } = new()
    {
        { 0, "Source" },
        { 44100, "44100hz" },
        { 48000, "48000hz" }
    };

    public ListItemCollectionView<int> BitrateList { get; } = new()
    {
        { 0, "Source" },
        { 96, "96 kbps" },
        { 128, "128 kbps" },
        { 192, "192 kbps" },
        { 256, "256 kbps" },
        { 320, "320 kbps" }
    };

    public ListItemCollectionView<FileExistsAction> FileExistsActionList { get; } = new()
    {
        { FileExistsAction.Ask, "Ask" },
        { FileExistsAction.Skip, "Skip" },
        { FileExistsAction.Overwrite, "Overwrite" },
        { FileExistsAction.Rename, "Rename" },
        { FileExistsAction.Cancel, "Cancel" }
    };

    /// <summary>
    /// Shows the advanced settings window.
    /// </summary>
    public RxCommandUnit ShowAdvancedSettings => _showAdvancedSettings ??= ReactiveCommand.CreateFromTask(ShowAdvancedSettingsImpl);
    private RxCommandUnit? _showAdvancedSettings;
    private Task ShowAdvancedSettingsImpl() =>
        _dialogService.ShowAdvancedSettingsAsync(this, Encoder.Settings);

    /// <summary>
    /// Starts the batch encoding job.
    /// </summary>
    public RxCommandUnit StartEncoding => _startEncoding ??= ReactiveCommand.CreateFromTask(StartEncodingImpl);
    private RxCommandUnit? _startEncoding;
    private Task StartEncodingImpl()
    {
        Encoder.ProcessingFiles.Clear();

        Encoder.Settings.Format = FormatsList.SelectedValue;
        Encoder.Settings.Bitrate = BitrateList.SelectedValue;
        Encoder.Settings.SampleRate = SampleRateList.SelectedValue;
        Encoder.FileExistsAction = FileExistsActionList.SelectedValue;
        Encoder.Settings.PitchTo = 432;

        return Encoder.RunAsync();
    }

    /// <summary>
    /// Cancels the batch encoding job.
    /// </summary>
    public RxCommandUnit StopEncoding => _stopEncoding ??= ReactiveCommand.Create(StopEncodingImpl);
    private RxCommandUnit? _stopEncoding;
    private void StopEncodingImpl() => Encoder.Cancel();
    
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
