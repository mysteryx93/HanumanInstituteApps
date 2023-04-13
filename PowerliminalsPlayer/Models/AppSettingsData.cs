using System.ComponentModel.DataAnnotations;
using HanumanInstitute.Apps;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

/// <summary>
/// Contains the PowerliminalsPlayer application settings.
/// </summary>
public class AppSettingsData : SettingsBase
{
    /// <summary>
    /// Gets or sets the list of folders in which to look for audio files.
    /// </summary>
    public ObservableCollection<string> Folders { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of saved presets.
    /// </summary>
    public ObservableCollection<PresetItem> Presets { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the folders section is expanded.
    /// </summary>
    [Reactive]
    public bool FoldersExpanded { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable the Anti-Alias filter.
    /// </summary>
    [Reactive]
    public bool AntiAlias { get; set; } = true;

    /// <summary>
    /// Gets or sets the Anti-Alias filter length. 
    /// </summary>
    [Reactive]
    [Range(minimum: 8, maximum: 128)]
    public int AntiAliasLength { get; set; } = 32;

    /// <summary>
    /// Gets or sets the performance or quality preference.
    /// </summary>
    [Reactive]
    [Range(minimum: 0, maximum: 5)]
    public int PerformanceQuality { get; set; } = 2;

    /// <summary>
    /// On Linux, sets the device output sample rate. On other platforms, null = auto-detected.
    /// </summary>
    [Reactive]
    public int? OutputSampleRate { get; set; }
}
