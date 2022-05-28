using System.ComponentModel;
using System.Windows.Input;
using ReactiveUI;

namespace HanumanInstitute.Converter432hz.ViewModels;

/// <summary>
/// Represents the playlist editor.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ISettingsProvider<AppSettingsData> _settings;

    public AppSettingsData AppData => _settings.Value;

    public MainViewModel(ISettingsProvider<AppSettingsData> settings)
    {
        _settings = settings.CheckNotNull(nameof(settings));

        _settings.Loaded += Settings_Loaded;
        Settings_Loaded(_settings, EventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets the height of the main window.
    /// </summary>
    public double WindowHeight
    {
        get => _settings.Value.Height;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._height, value, nameof(WindowHeight));
    }

    /// <summary>
    /// Gets or sets the width of the main window.
    /// </summary>
    public double WindowWidth
    {
        get => _settings.Value.Width;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._width, value, nameof(WindowWidth));
    }

    public PixelPoint WindowPosition
    {
        get => _settings.Value.Position;
        set => this.RaiseAndSetIfChanged(ref _settings.Value._position, value, nameof(WindowPosition));
    }

    /// <summary>
    /// After settings are loaded, get the list of playlists converted into PlaylistViewModel.
    /// </summary>
    private void Settings_Loaded(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(WindowHeight));
        this.RaisePropertyChanged(nameof(WindowWidth));
    }

    /// <summary>
    /// Before settings are saved, convert the list of PlaylistViewModel back into playlists.
    /// </summary>
    public ICommand SaveSettingsCommand => _saveSettingsCommand ??= ReactiveCommand.Create<CancelEventArgs>(OnSaveSettings);
    private ICommand? _saveSettingsCommand;
    private void OnSaveSettings(CancelEventArgs e)
    {
        _settings.Save();
    }
    
    public ICommand AddFile => _addFile ??= ReactiveCommand.Create(AddFileImpl);
    private ICommand _addFile;
    private Task AddFileImpl()
    {
        return Task.CompletedTask;
    }

    public ICommand AddFolder => _addFolder ??= ReactiveCommand.Create(AddFolderImpl);
    private ICommand _addFolder;
    private Task AddFolderImpl()
    {
        return Task.CompletedTask;
    }
}
