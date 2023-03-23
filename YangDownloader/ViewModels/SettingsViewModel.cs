using System.Text.Json.Serialization.Metadata;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class SettingsViewModel : SettingsViewModelBase<AppSettingsData>
{
    private readonly IDialogService _dialogService;
    private readonly IEncoderService _ffmpeg;
    private readonly IFileSystemService _fileSystem;

    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme,
        IDialogService dialogService, IEncoderService ffmpeg, IFileSystemService fileSystem, IJsonTypeInfoResolver? serializerContext) :
        base(settingsProvider, fluentTheme, serializerContext)
    {
        _dialogService = dialogService;
        _ffmpeg = ffmpeg;
        _fileSystem = fileSystem;
    }

    protected override bool Validate() => 
        TestDestinationPathImpl();

    protected override void RestoreDefaultImpl()
    {
        CheckForUpdateList.SelectedValue = UpdateInterval.Biweekly;
    }

    /// <summary>
    /// Shows the open folder dialog to select destination. 
    /// </summary>
    public RxCommandUnit BrowseDestination => _browseDestination ??= ReactiveCommand.CreateFromTask(BrowseDestinationImpl);
    private RxCommandUnit? _browseDestination;
    private async Task BrowseDestinationImpl()
    {
        var options = new OpenFolderDialogSettings()
        {
            Title = "Select destination folder",
            InitialDirectory = Settings.DestinationFolder
        };
        var result = await _dialogService.ShowOpenFolderDialogAsync(this, options).ConfigureAwait(false);
        if (result != null)
        {
            Settings.DestinationFolder = result.LocalPath;
            TestDestinationPathImpl();
        }
    }

    [Reactive]
    public string DestinationPathError { get; set; } = string.Empty;

    /// <summary>
    /// Tries to get FFmpeg version. FFmpegVersion and IsFFmpegPathValid will be set accordingly.
    /// </summary>
    /// <returns>Whether FFmpegPath is valid.</returns>
    public ReactiveCommand<Unit, bool> TestDestinationPath => _testDestinationPath ??= ReactiveCommand.Create(TestDestinationPathImpl);
    private ReactiveCommand<Unit, bool>? _testDestinationPath;
    private bool TestDestinationPathImpl()
    {
        if (string.IsNullOrWhiteSpace(Settings.DestinationFolder))
        {
            DestinationPathError = Resources.DestinationMissing;
        }
        else if (!_fileSystem.Directory.Exists(Settings.DestinationFolder))
        {
            DestinationPathError = Resources.DestinationDoesNotExist;
        }
        else
        {
            DestinationPathError = string.Empty;
        }
        return string.IsNullOrEmpty(DestinationPathError);
    }
}
