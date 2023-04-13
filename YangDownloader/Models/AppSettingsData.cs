using HanumanInstitute.BassAudio;
using HanumanInstitute.Downloads;

namespace HanumanInstitute.YangDownloader.Models;

/// <summary>
/// Contains the application settings and configured playlists.
/// </summary>
public class AppSettingsData : SettingsBase
{
    /// <summary>
    /// Gets or sets the destination folder where to save downloaded files.
    /// </summary>
    [Reactive]
    public string DestinationFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preferred video stream format.
    /// </summary>
    [Reactive]
    public StreamContainerOption PreferredVideo { get; set; }
    
    /// <summary>
    /// Gets or sets the preferred audio stream format.
    /// </summary>
    [Reactive]
    public StreamContainerOption PreferredAudio { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum quality to download.
    /// </summary>
    [Reactive]
    public int MaxQuality { get; set; }

    /// <summary>
    /// Gets or sets whether to re-encode audio.
    /// </summary>
    [Reactive]
    public bool EncodeAudio { get; set; }
    
    /// <summary>
    /// Gets or sets encoding settings.
    /// </summary>
    [Reactive]
    public EncodeSettings EncodeSettings { get; set; } = new();
}
