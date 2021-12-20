using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Avalonia;
using HanumanInstitute.Common.Services;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.Models;

/// <summary>
/// Contains the application settings and configured playlists.
/// </summary>
[Serializable]
[XmlRoot("Player432hz")]
public class AppSettingsData : ReactiveObject
{
    /// <summary>
    /// Gets or sets the list of configured playlists.
    /// </summary>
    [ValidateObject]
    [XmlElement("Playlist")]
    public List<SettingsPlaylistItem> Playlists
    {
        get => _playlists;
        set => this.RaiseAndSetIfChanged(ref _playlists, value, nameof(Playlists));
    }
    internal List<SettingsPlaylistItem> _playlists = new();
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
    /// <summary>
    /// Gets or sets the playback volume.
    /// </summary>
    [Range(0, 100)]
    public int Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value, nameof(Volume));
    } 
    internal int _volume = 100;
}
