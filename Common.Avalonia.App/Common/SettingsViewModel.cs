using System.Windows.Input;
using FluentAvalonia.Styling;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Shared ViewModel for the settings page.
/// </summary>
/// <typeparam name="TSettings">The data type of application settings.</typeparam>
public abstract class SettingsViewModel<TSettings> : ReactiveObject, IModalDialogViewModel, ICloseable
    where TSettings : SettingsDataBase, new()
{
    protected readonly FluentAvaloniaTheme _fluentTheme;
    protected readonly ISettingsProvider<TSettings> _settingsProvider;

    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel(ISettingsProvider<TSettings> settingsProvider, FluentAvaloniaTheme fluentTheme)
    {
        _settingsProvider = settingsProvider;
        _fluentTheme = fluentTheme;

        Settings = CloneSettings(_settingsProvider.Value);
        ThemeList.SelectedValue = Settings.Theme;
    }

    /// <inheritdoc />
    public event EventHandler? RequestClose;

    /// <inheritdoc />
    public bool? DialogResult { get; private set; }

    /// <summary>
    /// Returns a copy of application settings that are being edited.
    /// </summary>
    public TSettings Settings { get; }

    private bool SaveSettings()
    {
        if (Validate())
        {
            DialogResult = true;

            Settings.Theme = ThemeList.SelectedValue;
            _settingsProvider.Value = Settings;
            _fluentTheme.RequestedTheme = Settings.Theme.ToString();

            OnSaved();
            _settingsProvider.Save();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// When overriden in a derived class, customizes the way settings are cloned for editing.
    /// </summary>
    /// <param name="value">The settings object to clone.</param>
    /// <returns>The cloned object.</returns>
    protected virtual TSettings CloneSettings(TSettings value) => Cloning.ShallowClone(value);

    /// <summary>
    /// When overriden in a derived class, validates the data before saving.
    /// </summary>
    /// <returns>True if data is valid and ready to save, otherwise false to cancel the save.</returns>
    protected virtual bool Validate() => true;

    /// <summary>
    /// Occurs after settings are saved.
    /// </summary>
    protected virtual void OnSaved() { }

    /// <summary>
    /// Gets the list of themes for display.
    /// </summary>
    public ListItemCollectionView<AppTheme> ThemeList { get; } = new()
    {
        { AppTheme.Light, "Light" },
        { AppTheme.Dark, "Dark" },
        { AppTheme.HighContrast, "HighContrast" }
    };

    /// <summary>
    /// Restores default settings.
    /// </summary>
    public ICommand RestoreDefault => _restoreDefault ??= ReactiveCommand.Create(RestoreDefaultImpl);
    private ICommand? _restoreDefault;
    /// <summary>
    /// When overriden in a derived class, restores default settings.
    /// </summary>
    protected abstract void RestoreDefaultImpl();

    /// <summary>
    /// Applies changes without closing the window.
    /// </summary>
    public ICommand Apply => _apply ??= ReactiveCommand.Create(ApplyImpl);
    private ICommand? _apply;
    private void ApplyImpl() => SaveSettings();

    /// <summary>
    /// Saves changes and closes the window.
    /// </summary>
    public ICommand Ok => _ok ??= ReactiveCommand.Create(OkImpl);
    private ICommand? _ok;
    private void OkImpl()
    {
        if (SaveSettings())
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Closes the window without saving.
    /// </summary>
    public ICommand Cancel => _cancel ??= ReactiveCommand.Create(CancelImpl);
    private ICommand? _cancel;
    private void CancelImpl() => RequestClose?.Invoke(this, EventArgs.Empty);
}
