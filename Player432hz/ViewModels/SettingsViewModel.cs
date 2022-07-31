using System.Windows.Input;
using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

public class SettingsViewModel : ReactiveObject, IModalDialogViewModel, ICloseable
{
    private readonly FluentAvaloniaTheme _fluentTheme;
    private readonly IPlaylistPlayer _playlistPlayer;
    private readonly ISettingsProvider<AppSettingsData> _settingsProvider;

    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, FluentAvaloniaTheme fluentTheme, IPlaylistPlayer playlistPlayer)
    {
        _settingsProvider = settingsProvider;
        _fluentTheme = fluentTheme;
        _playlistPlayer = playlistPlayer;
        
        Settings = _settingsProvider.Value.Clone();
        ThemeList.SelectedValue = Settings.Theme;
    }
    
    public event EventHandler? RequestClose;
    
    public bool? DialogResult { get; private set; }

    protected AppSettingsData Settings { get; }

    private void SaveSettings()
    {
        DialogResult = true;

        Settings.Theme = ThemeList.SelectedValue;
        _settingsProvider.Value = Settings;
        _fluentTheme.RequestedTheme = Settings.Theme.ToString();
        
        _playlistPlayer.ApplySettings();
        _settingsProvider.Save();
    }
    
    public ListItemCollectionView<AppTheme> ThemeList { get; } = new()
    {
        { AppTheme.Light, "Light" },
        { AppTheme.Dark, "Dark" },
        { AppTheme.HighContrast, "HighContrast" }
    };
    
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
        SaveSettings();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Closes the window without saving.
    /// </summary>
    public ICommand Cancel => _cancel ??= ReactiveCommand.Create(CancelImpl);
    private ICommand? _cancel;
    private void CancelImpl() => RequestClose?.Invoke(this, EventArgs.Empty);
}
