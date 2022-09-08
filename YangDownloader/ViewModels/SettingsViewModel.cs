using System.ComponentModel;
using AngleSharp.Media;
using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    private readonly IDialogService _dialogService;
    private readonly IEncoderService _ffmpeg;
    private readonly IFileSystemService _fileSystem;

    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme,
        IDialogService dialogService, IEncoderService ffmpeg, IFileSystemService fileSystem) :
        base(settingsProvider, fluentTheme)
    {
        _dialogService = dialogService;
        _ffmpeg = ffmpeg;
        _fileSystem = fileSystem;

        TestFFmpegImpl();
    }

    protected override bool Validate() => 
        TestDestinationPathImpl() & TestFFmpegImpl();

    protected override void RestoreDefaultImpl()
    {
        CheckForUpdateList.SelectedValue = UpdateInterval.Biweekly;
        Settings.FFmpegPath = "ffmpeg";
        TestFFmpegImpl();
    }

    /// <summary>
    /// When the window is closed, sets FFmpeg path in IEncoderService whether settings are saved or not.
    /// </summary>
    public ReactiveCommand<CancelEventArgs, Unit> Closed => _closed ??= ReactiveCommand.Create<CancelEventArgs>(ClosedImpl);
    private ReactiveCommand<CancelEventArgs, Unit>? _closed;
    private void ClosedImpl(CancelEventArgs e) => 
        _ffmpeg.Processes.Paths.FFmpeg = _settingsProvider.Value.FFmpegPath;

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
            Settings.DestinationFolder = result;
            TestDestinationPathImpl();
        }
    }

    /// <summary>
    /// Shows the open file dialog to locate FFmpeg executable. 
    /// </summary>
    public RxCommandUnit BrowseFFmpeg => _browseFFmpeg ??= ReactiveCommand.CreateFromTask(BrowseFFmpegImpl);
    private RxCommandUnit? _browseFFmpeg;
    private async Task BrowseFFmpegImpl()
    {
        var options = new OpenFileDialogSettings()
        {
            Title = "Locate FFmpeg Executable",
            InitialFile = Settings.FFmpegPath
        };
        var result = await _dialogService.ShowOpenFileDialogAsync(this, options).ConfigureAwait(false);
        if (result != null)
        {
            Settings.FFmpegPath = result;
            TestFFmpegImpl();
        }
    }

    [Reactive]
    public bool IsFFmpegPathValid { get; set; }
    
    [Reactive]
    public string FFmpegVersion { get; set; } = string.Empty;

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
    
    /// <summary>
    /// Tries to get FFmpeg version. FFmpegVersion and IsFFmpegPathValid will be set accordingly.
    /// </summary>
    /// <returns>Whether FFmpegPath is valid.</returns>
    public ReactiveCommand<Unit, bool> TestFFmpeg => _testFFmpeg ??= ReactiveCommand.Create(TestFFmpegImpl);
    private ReactiveCommand<Unit, bool>? _testFFmpeg;
    private bool TestFFmpegImpl()
    {
        var version = GetFFmpegVersion();
        IsFFmpegPathValid = version.HasValue();
        FFmpegVersion = version.HasValue() ?
            "Found FFmpeg " + version :
            "FFmpeg not found";
        return IsFFmpegPathValid;
    }
    
    private string GetFFmpegVersion()
    {
        try
        {
            _ffmpeg.Processes.Paths.FFmpeg = Settings.FFmpegPath;
            var options = new ProcessOptionsEncoder(ProcessDisplayMode.None) { Timeout = TimeSpan.FromSeconds(1) };
            var version = _ffmpeg.GetMediaInfoReader(this).GetVersion(options);
            var prefix = "ffmpeg version ";
            if (version.StartsWith(prefix))
            {
                var endPos = version.IndexOf(' ', prefix.Length + 1);
                if (endPos > -1)
                {
                    return version.Substring(prefix.Length, endPos - prefix.Length);
                }
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
        return string.Empty;
    }
}
