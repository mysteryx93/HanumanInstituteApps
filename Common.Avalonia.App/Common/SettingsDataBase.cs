using System.Text.Json.Serialization;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HanumanInstitute.Common.Avalonia.App;

public class SettingsDataBase : ReactiveObject
{
    /// <summary>
    /// Gets or sets the width of the main window.
    /// </summary>
    [Reactive]
    public double Width { get; set; }
    
    /// <summary>
    /// Gets or sets the height of the main window.
    /// </summary>
    [Reactive]
    public double Height { get; set; }
    
    /// <summary>
    /// Gets or sets the position of the main window.
    /// </summary>
    [Reactive]
    public PixelPoint Position { get; set; }

    /// <summary>
    /// Gets or sets whether to display the About window on startup.
    /// </summary>
    [Reactive]
    public bool ShowInfoOnStartup { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the visual theme: Light or Dark.
    /// </summary>
    [Reactive]
    public AppTheme Theme { get; set; }

    /// <summary>
    /// Gets or sets when to automatically check for updates.
    /// </summary>
    [Reactive]
    public UpdateInterval CheckForUpdates { get; set; } = UpdateInterval.Weekly;
    
    /// <summary>
    /// Gets or sets the date of the last check for updates.
    /// </summary>
    [Reactive]
    public DateTime? LastCheckForUpdate { get; set; }
    
    /// <summary>
    /// Gets or sets the date of the last startup popup when using no license.
    /// </summary>
    [Reactive]
    public DateTime? LastShowInfo { get; set; }
    
    /// <summary>
    /// Gets or sets the license key.
    /// </summary>
    [Reactive]
    public string? LicenseKey { get; set; }

    /// <summary>
    /// Gets or sets whether the license key is valid.
    /// </summary>
    [Reactive]
    [JsonIgnore]
    public bool IsLicenseValid { get; set; }
}
