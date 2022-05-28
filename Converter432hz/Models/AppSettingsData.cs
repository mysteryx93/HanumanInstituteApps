using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using ReactiveUI;

namespace HanumanInstitute.Converter432hz.Models;

/// <summary>
/// Contains the application settings and configured playlists.
/// </summary>
[Serializable]
[XmlRoot("Converter432hz")]
public class AppSettingsData : ReactiveObject
{
    /// <summary>
    /// Gets or sets the width of the main window.
    /// </summary>
    [Range(560, 10000)]
    public double Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value, nameof(Width));
    }
    internal double _width = 583;
    
    /// <summary>
    /// Gets or sets the height of the main window.
    /// </summary>
    [Range(240, 10000)]
    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value, nameof(Height));
    }
    internal double _height = 390;
    
    /// <summary>
    /// Gets or sets the position of the main window.
    /// </summary>
    public PixelPoint Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value, nameof(Position));
    }
    internal PixelPoint _position;
}
