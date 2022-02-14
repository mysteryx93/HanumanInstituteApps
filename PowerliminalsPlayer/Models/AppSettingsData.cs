using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Avalonia;
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
    private double _width;
    
    /// <summary>
    /// Gets or sets the main window height.
    /// </summary>
    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }
    private double _height;

    /// <summary>
    /// Creates a copy of the SettingsFile class.
    /// </summary>
    /// <returns>The copied object.</returns>
    public AppSettingsData Clone() => (AppSettingsData)MemberwiseClone();
}
