using Avalonia.Controls;
using HanumanInstitute.Apps.AdRotator;
using ReactiveUI;

// Trimming fails if MainViewModelBase is in a separate assembly.
// Copied here as a work-around.
// https://github.com/AvaloniaUI/Avalonia/issues/10494

namespace HanumanInstitute.YangDownloader.ViewModels;

/// <summary>
/// Shared ViewModel for the information page.
/// </summary>
/// <typeparam name="TSettings">The data type of application settings.</typeparam>
public abstract class AboutViewModelBase<TSettings> : ReactiveObject, IModalDialogViewModel, ICloseable
    where TSettings : SettingsBase, new()
{
    private readonly IEnvironmentService _environment;
    private readonly ISettingsProvider<TSettings> _settings;
    private readonly IHanumanInstituteHttpClient _httpClient;
    private readonly ILicenseValidator _licenseValidator;
    private readonly IDialogService _dialogService;
    private readonly IAdRotatorViewModel _adRotator;

    /// <inheritdoc />
    public event EventHandler? RequestClose;
    /// <inheritdoc />
    public bool? DialogResult { get; } = true;
    /// <summary>
    /// Returns information about the application.
    /// </summary>
    public IAppInfo AppInfo { get; }
    
    /// <summary>
    /// Initializes a new instance of the AboutViewModel class.
    /// </summary>
    protected AboutViewModelBase(IAppInfo appInfo, IEnvironmentService environment, ISettingsProvider<TSettings> settings,
        IHanumanInstituteHttpClient httpClient, ILicenseValidator licenseValidator, IDialogService dialogService, IAdRotatorViewModel adRotator)
    {
        AppInfo = appInfo;
        _environment = environment;
        _settings = settings;
        _httpClient = httpClient;
        _licenseValidator = licenseValidator;
        _dialogService = dialogService;
        _adRotator = adRotator;
        // ReSharper disable once VirtualMemberCallInConstructor
        _license = _settings.Value.LicenseKey;

        // Start in constructor to save time.
        if (!Design.IsDesignMode)
        {
            CheckForUpdates.Execute().Subscribe();
        }
    }

    /// <summary>
    /// Returns application settings.
    /// </summary>
    public SettingsBase Settings => _settings.Value;

    /// <summary>
    /// Returns the version of the application.
    /// </summary>
    public Version AppVersion => _environment.AppVersion;

    /// <summary>
    /// Gets or sets whether to display the about screen on startup.
    /// </summary>
    public bool ShowInfoOnStartup
    {
        get => Settings.ShowInfoOnStartup;
        set => Settings.ShowInfoOnStartup = value;
    }

    /// <summary>
    /// Gets or sets the license key. 
    /// </summary>
    public string? License
    {
        get => _license;
        set
        {
            this.RaiseAndSetIfChanged(ref _license, value?.Trim());
            Settings.IsLicenseValid = _license.HasValue() && _licenseValidator.Validate(_license);
            if (Settings.IsLicenseValid)
            {
                Settings.LicenseKey = _license;
            }
            _adRotator.Enabled = !Settings.IsLicenseValid;
        }
    }
    private string? _license;

    /// <summary>
    /// Returns the text to display on the Check For Updates link.
    /// </summary>
    public string CheckForUpdateText
    {
        get => _checkForUpdateText;
        set => this.RaiseAndSetIfChanged(ref _checkForUpdateText, value);
    }
    private string _checkForUpdateText = "Checking for updates...";

    /// <summary>
    /// Checks GitHub releases for an application update.
    /// </summary>
    public RxCommandUnit CheckForUpdates => _checkForUpdates ??= ReactiveCommand.CreateFromTask(CheckForUpdatesImpl);
    private RxCommandUnit? _checkForUpdates;
    private async Task CheckForUpdatesImpl()
    {
        CheckForUpdateText = "Checking for updates...";
        var version = await _httpClient.QueryVersionAsync();
        if (version != null)
        {
            CheckForUpdateText = version.LatestVersion > _environment.AppVersion ?
                $"v{version} is available!" :
                "You have the latest version";
            if (version.AdsLastUpdated > _adRotator.AdInfo.LastUpdated)
            {
                await _adRotator.LoadFromServerAsync();
            }
        }
        else
        {
            CheckForUpdateText = "Check for updates failed";
        }
    }

    /// <summary>
    /// Closes the window.
    /// </summary>
    public RxCommandUnit Close => _close ??= ReactiveCommand.Create(CloseImpl);
    private RxCommandUnit? _close;
    private void CloseImpl()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
        _settings.Save();
    }
}
