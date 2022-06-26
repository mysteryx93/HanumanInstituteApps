using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
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

    public AppSettingsData AppData => _settings.Value;

    public MainViewModel(ISettingsProvider<AppSettingsData> settings, IEncoderService encoder, IDialogService dialogService,
        IFileSystemService fileSystem, IFileLocator fileLocator, IAppPathService appPath, IEnvironmentService environment)
    {
        _settings = settings.CheckNotNull(nameof(settings));
        Encoder = encoder;
        Encoder.Owner = this;
        _dialogService = dialogService;
        _fileSystem = fileSystem;
        _fileLocator = fileLocator;
        _appPath = appPath;
        _environment = environment;
        
        Encoder.Settings.PitchTo = 432;
        Encoder.Settings.MaxThreads = Math.Min(64, _environment.ProcessorCount);
        FormatsList.SelectedValue = EncodeFormat.Mp3;
        BitrateList.SelectedValue = 0;
        SampleRateList.SelectedValue = 48000;
        FileExistsActionList.SelectedValue = FileExistsAction.Ask;

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

        FilesLeftObservable = Encoder.Sources.AsObservableChangeSet().Select(x => x switch
        {
            IChangeSet<FolderItem> folder => folder.Select(f => f.Item.Current.Files.Count).Sum(),
            _ => 1
        }).Sum();
        this.WhenAnyObservable(x => x.FilesLeftObservable).ToProperty(this, x => x.FilesLeft);
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
    public bool IsBitrateVisible =>  _isBitrateVisible.Value;  
    private readonly ObservableAsPropertyHelper<bool> _isBitrateVisible;
    
    /// <summary>
    /// Gets whether SampleRate control should be visible.
    /// </summary>
    public bool IsSampleRateVisible =>  _isSampleRateVisible.Value;  
    private readonly ObservableAsPropertyHelper<bool> _isSampleRateVisible;

    /// <summary>
    /// Gets whether QualitySpeed slider should be visible.
    /// </summary>
    public bool IsQualitySpeedVisible =>  _isQualitySpeedVisible.Value;  
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
                new FileFilter("All files", "*"),
                new FileFilter("Audio files", _appPath.AudioExtensions)
            }
        };
        var files = await _dialogService.ShowOpenFilesDialogAsync(this, null);
        foreach (var item in files)
        {
            Encoder.Sources.Add(new FileItem(item, _fileSystem.Path.GetFileName(item)));
        }
    }

    public ICommand AddFolder => _addFolder ??= ReactiveCommand.CreateFromTask(AddFolderImpl);
    private ICommand? _addFolder;
    private async Task AddFolderImpl()
    {
        var settings = new OpenFolderDialogSettings() { Title = "Convert all audio files in folder" };
        var folder = await _dialogService.ShowOpenFolderDialogAsync(this, settings);
        if (folder != null)
        {
            var folderItem = new FolderItem(folder, folder);
            foreach (var file in _fileLocator.GetAudioFiles(folder))
            {
                folderItem.Files.Add(file);
            }
            Encoder.Sources.Add(folderItem);
        }
    }

    public ICommand RemoveFile => _removeFile ??= ReactiveCommand.Create(RemoveFileImpl);
    private ICommand? _removeFile;
    private void RemoveFileImpl()
    {
        if (SourcesSelectedIndex > -1 && SourcesSelectedIndex < Encoder.Sources.Count)
        {
            Encoder.Sources.RemoveAt(SourcesSelectedIndex);
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
