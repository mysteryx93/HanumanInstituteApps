using Avalonia.Threading;
using HanumanInstitute.Common.Services;
using ReactiveUI;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Base implementation of the main ViewModel with shared features.
/// </summary>
/// <typeparam name="TSettings">The data type of application settings.</typeparam>
public abstract class BaseWithSettings<TSettings> : ReactiveObject, IDisposable
    where TSettings : SettingsDataBase, new()
{
    protected readonly ISettingsProvider<TSettings> _settings;

    /// <summary>
    /// Initializes a new instance of the MainViewModelBase class.
    /// </summary>
    protected BaseWithSettings(ISettingsProvider<TSettings> settings)
    {
        _settings = settings;
        
        _settings.Changed += Settings_Loaded;
        _settings.Saving += Settings_Saving;
        // Dispatcher.UIThread.Post(ConvertFromSettings);
        Task.Run(ConvertFromSettings);
    }
    
    private void Settings_Loaded(object? sender, EventArgs e)
    {
        ConvertFromSettings();
        this.RaisePropertyChanged(nameof(Settings));
        ApplySettings();
    }
    
    private void Settings_Saving(object? sender, EventArgs e)
    {
        ConvertToSettings();
        ApplySettings();
    }

    /// <summary>
    /// Exposes application settings.
    /// </summary>
    public TSettings Settings => _settings.Value;
    
    /// <summary>
    /// Converts settings into application data after loading.
    /// </summary>
    protected virtual void ConvertFromSettings() { }
    
    /// <summary>
    /// Converts data back to settings before saving changes.
    /// </summary>
    protected virtual void ConvertToSettings() { }
    
    /// <summary>
    /// Applies settings after they are modified.
    /// </summary>
    protected virtual void ApplySettings() { }

    private bool _disposed;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _settings.Changed -= Settings_Loaded;
                _settings.Saving -= Settings_Saving;
            }
            _disposed = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
