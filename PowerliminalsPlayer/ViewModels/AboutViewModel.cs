using System.Windows.Input;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

public class AboutViewModel : ReactiveObject, IModalDialogViewModel, ICloseable
{
    private readonly IEnvironmentService _environment;
    private readonly ISettingsProvider<AppSettingsData> _settings;
    private readonly IProcessService _processService;
    private readonly IUpdateService _updateService;
    
    /// <inheritdoc />
    public event EventHandler? RequestClose;
    /// <inheritdoc />
    public bool? DialogResult { get; } = true;

    public AboutViewModel(IEnvironmentService environment, IProcessService processService, ISettingsProvider<AppSettingsData> settings, IUpdateService updateService)
    {
        _environment = environment;
        _processService = processService;
        _settings = settings;
        _updateService = updateService;
        _updateService.FileFormat = "PowerliminalsPlayer-{0}_Win_x64.zip";
        
        // Start in constructor to save time.
        if (!Design.IsDesignMode)
        {
            CheckForUpdates.Execute();
        }
    }

    /// <summary>
    /// Returns the name of the application.
    /// </summary>
    public string AppName => "Powerliminals Player";

    /// <summary>
    /// Returns the description of the application.
    /// </summary>
    public string AppDescription => "Plays multiple audios simultaneously at varying speeds";

    /// <summary>
    /// Returns the version of the application.
    /// </summary>
    public Version AppVersion => _environment.AppVersion;

    /// <summary>
    /// Gets or sets whether to display the About window on startup.
    /// </summary>
    public bool ShowInfoOnStartup
    {
        get => _settings.Value.ShowInfoOnStartup;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._showInfoOnStartup, value);
    }
    
    /// <summary>
    /// Returns the text to display on the Check For Updates link.
    /// </summary>
    [Reactive]
    public string CheckForUpdateText { get; set; } = "Checking for updates...";

    /// <summary>
    /// Checks GitHub releases for an application update.
    /// </summary>
    public ICommand CheckForUpdates => _checkForUpdates ??= ReactiveCommand.CreateFromTask(CheckForUpdatesImpl);
    private ICommand? _checkForUpdates;
    private async Task CheckForUpdatesImpl()
    {
        CheckForUpdateText = "Checking for updates...";
        var version = await _updateService.GetLatestVersionAsync();
        CheckForUpdateText = version > _environment.AppVersion ?
            $"v{version} is available!" :
            version != null ?
                "You have the latest version" :
                "Could not get update information";
    }

    /// <summary>
    /// Opens specified URL in the default browser.
    /// </summary>
    public ICommand OpenBrowser => _openBrowser ??= ReactiveCommand.Create<string>(OpenBrowserImpl);
    private ICommand? _openBrowser;
    private void OpenBrowserImpl(string url) => _processService.OpenBrowserUrl(url);
    
    /// <summary>
    /// Closes the window.
    /// </summary>
    public ICommand Close => _close ??= ReactiveCommand.Create(CloseImpl);
    private ICommand? _close;
    private void CloseImpl()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
