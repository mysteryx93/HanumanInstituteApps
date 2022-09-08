using HanumanInstitute.Common.Services;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Base implementation of the main ViewModel with shared features.
/// </summary>
/// <typeparam name="TSettings">The data type of application settings.</typeparam>
public abstract class MainViewModelBase<TSettings> : BaseWithSettings<TSettings>, ICloseable, IViewLoaded, IViewClosed
    where TSettings : SettingsDataBase, new()
{
    private IAppUpdateService _appUpdateService;

    /// <summary>
    /// Initializes a new instance of the MainViewModelBase class.
    /// </summary>
    protected MainViewModelBase(ISettingsProvider<TSettings> settings, IAppUpdateService appUpdateService) :
        base(settings)
    {
        _appUpdateService = appUpdateService;
    }
    
    /// <inheritdoc />
    public event EventHandler? RequestClose;

    /// <summary>
    /// On startup, show About window and/or check for updates. 
    /// </summary>
    public virtual async void OnLoaded()
    {
        if (_settings.Value.ShowInfoOnStartup)
        {
            await Task.Delay(400).ConfigureAwait(true);
            ShowAbout.Execute().Subscribe();
            _settings.Save();
        }
        else
        {
            await _appUpdateService.CheckForUpdatesAsync(this).ConfigureAwait(false);
        }
        _appUpdateService = null!; // We don't need anymore.
    }
    
    /// <summary>
    /// Shows the About window.
    /// </summary>
    public RxCommandUnit ShowAbout => _showAbout ??= ReactiveCommand.CreateFromTask(ShowAboutImplAsync);
    private RxCommandUnit? _showAbout;
    /// <summary>
    /// Shows the About window.
    /// </summary>
    protected virtual Task ShowAboutImplAsync() => Task.CompletedTask;

    /// <summary>
    /// Shows the Settings window.
    /// </summary>
    public RxCommandUnit ShowSettings => _showSettings ??= ReactiveCommand.CreateFromTask(ShowSettingsImplAsync);
    private RxCommandUnit? _showSettings;
    /// <summary>
    /// Shows the Settings window.
    /// </summary>
    protected virtual Task ShowSettingsImplAsync() => Task.CompletedTask;
    
    /// <summary>
    /// Unregister events when the View is closed.
    /// </summary>
    public virtual void OnClosed()
    {
        _settings.Save();
        Dispose();
    }
}
