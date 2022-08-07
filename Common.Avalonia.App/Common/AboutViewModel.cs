using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Shared ViewModel for the information page.
/// </summary>
/// <typeparam name="TSettings">The data type of application settings.</typeparam>
public abstract class AboutViewModel<TSettings> : ReactiveObject, IModalDialogViewModel, ICloseable
    where TSettings : SettingsDataBase, new()
{
    private readonly IEnvironmentService _environment;
    private readonly ISettingsProvider<TSettings> _settings;
    private readonly IProcessService _processService;
    private readonly IUpdateService _updateService;

    /// <inheritdoc />
    public event EventHandler? RequestClose;
    /// <inheritdoc />
    public bool? DialogResult { get; } = true;
    
    /// <summary>
    /// Initializes a new instance of the AboutViewModel class.
    /// </summary>
    protected AboutViewModel(IEnvironmentService environment, IProcessService processService, ISettingsProvider<TSettings> settings,
        IUpdateService updateService)
    {
        _environment = environment;
        _processService = processService;
        _settings = settings;
        _updateService = updateService;
        // ReSharper disable once VirtualMemberCallInConstructor
        _updateService.FileFormat = UpdateFileFormat;

        // Start in constructor to save time.
        if (!Design.IsDesignMode)
        {
            CheckForUpdates.Execute().Subscribe();
        }
    }

    /// <summary>
    /// Returns application settings.
    /// </summary>
    public SettingsDataBase AppSettings => _settings.Value;

    /// <summary>
    /// Returns the format of files released on GitHub. 
    /// </summary>
    public abstract string UpdateFileFormat { get; }

    /// <summary>
    /// Returns the name of the application.
    /// </summary>
    public abstract string AppName { get; }

    /// <summary>
    /// Returns the description of the application.
    /// </summary>
    public abstract string AppDescription { get; }

    /// <summary>
    /// Returns the version of the application.
    /// </summary>
    public Version AppVersion => _environment.AppVersion;

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
        var version = await _updateService.GetLatestVersionAsync();
        CheckForUpdateText = version > _environment.AppVersion ?
            $"v{version} is available!" :
            "You have the latest version";
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
