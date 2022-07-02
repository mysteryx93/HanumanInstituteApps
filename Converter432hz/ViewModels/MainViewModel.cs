using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using DynamicData.Aggregation;
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

    public AppSettingsData AppData => _settings.Value;

    public MainViewModel(ISettingsProvider<AppSettingsData> settings, IEncoderService encoder, IDialogService dialogService,
        IFileSystemService fileSystem, IFileLocator fileLocator, IAppPathService appPath, IEnvironmentService environment,
        IPitchDetector pitchDetector)
    {
        _settings = settings.CheckNotNull(nameof(settings));
        Encoder = encoder;
        Encoder.Owner = this;
        _dialogService = dialogService;
        _fileSystem = fileSystem;
        _fileLocator = fileLocator;
        _appPath = appPath;
        _environment = environment;
        _pitchDetector = pitchDetector;

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

        // FilesLeftObservable = Encoder.Sources.AsObservableChangeSet().Transform(x => x switch
        // {
        //     FolderItem folder => folder.Files.Count,
        //     _ => 1
        // }).Sum(x => x);
        // this.WhenAnyObservable(x => x.FilesLeftObservable).ToProperty(this, x => x.FilesLeft);
        //
        // FilesCompletedCount = Encoder.ProcessingFiles.Connect().Filter(x => x.Status == EncodeStatus.Completed).Count();
        // var combined = new SourceList<IObservable<int>>();
        // var sourceObserve = Encoder.Sources.Connect();
        // combined.Add(sourceObserve.Filter(x => x is not FolderItem).Count());
        // combined.Add(SumEx.Sum(sourceObserve.Filter(x => x is FolderItem), x =>((FolderItem)x).Files.Count));
    }

    public int FilesLeft { get; private set; }

    public IObservable<int> FilesLeftObservable { get; private set; }

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

    // /// <summary>
    // /// Gets the quantity of files completed.
    // /// </summary>
    // public IObservable<int> FilesCompletedCount { get; }

    /// <summary>
    /// The encoder service.
    /// </summary>
    public IEncoderService Encoder { get; }

    public PixelPoint WindowPosition
    {
        get => _settings.Value.Position;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._position, value, nameof(WindowPosition));
    }

    /// <summary>
    /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
    /// </summary>
    public ICommand SaveSettingsCommand => _saveSettingsCommand ??= ReactiveCommand.Create<CancelEventArgs>(OnSaveSettings);
    private ICommand? _saveSettingsCommand;
    private void OnSaveSettings(CancelEventArgs e) => _settings.Save();

    [Reactive] public int SourcesSelectedIndex { get; set; }

    public ICommand AddFiles => _addFiles ??= ReactiveCommand.CreateFromTask(AddFilesImpl);
    private ICommand? _addFiles;
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

    public ICommand AddFolder => _addFolder ??= ReactiveCommand.CreateFromTask(AddFolderImpl);
    private ICommand? _addFolder;
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
        }
    }

    public ICommand RemoveFile => _removeFile ??= ReactiveCommand.Create(RemoveFileImpl, 
        this.WhenAnyValue(x => x.SourcesSelectedIndex, index => index > -1));
    private ICommand? _removeFile;
    private void RemoveFileImpl()
    {
        if (SourcesSelectedIndex > -1 && SourcesSelectedIndex < Encoder.Sources.Count)
        {
            var sel = SourcesSelectedIndex;
            Encoder.Sources.RemoveAt(sel);
            SourcesSelectedIndex = -1;
            SourcesSelectedIndex = sel >= Encoder.Sources.Count ? Encoder.Sources.Count - 1 : sel; 
        }
    }

    public ICommand BrowseDestination => _browseDestination ??= ReactiveCommand.CreateFromTask(BrowseDestinationImpl);
    private ICommand? _browseDestination;
    private async Task BrowseDestinationImpl()
    {
        var settings = new OpenFolderDialogSettings() { Title = "Destination" };
        var folder = await _dialogService.ShowOpenFolderDialogAsync(this, settings);
        if (folder != null)
        {
            Encoder.Destination = folder;
        }
    }

    public ListItemCollectionView<EncodeFormat> FormatsList { get; } = new()
    {
        { EncodeFormat.Mp3, "MP3" },
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
    public ICommand ShowAdvancedSettings => _showAdvancedSettings ??= ReactiveCommand.CreateFromTask(ShowAdvancedSettingsImpl);
    private ICommand? _showAdvancedSettings;
    private Task ShowAdvancedSettingsImpl() =>
        _dialogService.ShowAdvancedSettingsAsync(this, Encoder.Settings);

    /// <summary>
    /// Starts the batch encoding job.
    /// </summary>
    public ICommand StartEncoding => _startEncoding ??= ReactiveCommand.CreateFromTask(StartEncodingImpl);
    private ICommand? _startEncoding;
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
    public ICommand StopEncoding => _stopEncoding ??= ReactiveCommand.Create(StopEncodingImpl);
    private ICommand? _stopEncoding;
    private void StopEncodingImpl() => Encoder.Cancel();
}
