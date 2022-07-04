using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Avalonia;
using HanumanInstitute.Common.Avalonia;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

/// <summary>
/// Contains the PowerliminalsPlayer application settings.
/// </summary>
[XmlRoot("PowerliminalsPlayer")]
public class AppSettingsData : ReactiveObject
{
    /// <summary>
    /// Gets or sets the list of folders in which to look for audio files.
    /// </summary>
    [XmlElement("Folder")]
    public ObservableCollection<string> Folders { get; } = new ObservableCollection<string>();

    /// <summary>
    /// Gets or sets the list of saved presets.
    /// </summary>
    //[XmlElement("Preset")]
    public ObservableCollection<PresetItem> Presets { get; } = new ObservableCollection<PresetItem>();

    /// <summary>
    /// Gets or sets whether the folders section is expanded.
    /// </summary>
    public bool FoldersExpanded
    {
        get => _foldersExpanded;
        set => this.RaiseAndSetIfChanged(ref _foldersExpanded, value);
    }
    private bool _foldersExpanded = true;

    /// <summary>
    /// Gets or sets the main window width.
    /// </summary>
    public double Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }
    private double _width = 730;

    /// <summary>
    /// Gets or sets the main window height.
    /// </summary>
    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }
    private double _height = 410;

    public PositionExtension.PositionPoint Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }
    private PositionExtension.PositionPoint _position;

    /// <summary>
    /// Gets or sets whether to display the About window on startup.
    /// </summary>
    public bool ShowInfoOnStartup
    {
        get => _showInfoOnStartup;
        set => this.RaiseAndSetIfChanged(ref _showInfoOnStartup, value);
    }
    internal bool _showInfoOnStartup = true;
    
    // [XmlElement("Position")]
    // public PositionExtension.PositionPoint PositionStore
    // {
    //     get => _positionStore;
    //     set
    //     {
    //         if (value.X != _positionStore.X || value.Y != _positionStore.Y)
    //         {
    //             this.RaiseAndSetIfChanged(ref _positionStore, value);
    //             Position = new PixelPoint(value.X, value.Y);
    //         }
    //     }
    // }
    // private PositionExtension.PositionPoint _positionStore;

    /// <summary>
    /// Creates a copy of the SettingsFile class.
    /// </summary>
    /// <returns>The copied object.</returns>
    public AppSettingsData Clone() => (AppSettingsData)MemberwiseClone();


}
