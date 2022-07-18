using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using HanumanInstitute.BassAudio;
using HanumanInstitute.Downloads;
using ReactiveUI;

namespace HanumanInstitute.YangDownloader.Models;

/// <summary>
/// Contains the application settings and configured playlists.
/// </summary>
[Serializable]
[XmlRoot("Player432hz")]
public class AppSettingsData : ReactiveObject
{
    /// <summary>
    /// Gets or sets the width of the main window.
    /// </summary>
    [Range(560, 10000)]
    public double Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }
    internal double _width = 583;
    
    /// <summary>
    /// Gets or sets the height of the main window.
    /// </summary>
    [Range(240, 10000)]
    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }
    internal double _height = 390;
    
    /// <summary>
    /// Gets or sets the position of the main window.
    /// </summary>
    public PixelPoint Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }
    internal PixelPoint _position;

    /// <summary>
    /// Gets or sets whether to display the About window on startup.
    /// </summary>
    public bool ShowInfoOnStartup
    {
        get => _showInfoOnStartup;
        set => this.RaiseAndSetIfChanged(ref _showInfoOnStartup, value);
    }
    internal bool _showInfoOnStartup = true;

    /// <summary>
    /// Gets or sets the preferred video stream format.
    /// </summary>
    public StreamContainerOption PreferredVideo { get; set; }
    
    /// <summary>
    /// Gets or sets the preferred audio stream format.
    /// </summary>
    public StreamContainerOption PreferredAudio { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum quality to download.
    /// </summary>
    public int MaxQuality { get; set; }
    
    /// <summary>
    /// Gets or sets encoding settings.
    /// </summary>
    public EncodeSettings EncodeSettings { get; set; } = new EncodeSettings();

    /// <summary>
    /// Gets or sets whether to re-encode audio.
    /// </summary>
    public bool EncodeAudio { get; set; }
}
