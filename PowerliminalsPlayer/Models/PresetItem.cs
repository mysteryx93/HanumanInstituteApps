using System;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using HanumanInstitute.Common.Avalonia;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

public class PresetItem : ReactiveObject
{
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    private string _name = string.Empty;
        
    [XmlElement("File")]
    public ObservableCollectionWithRange<FileItem> Files { get; } = new();

    public PresetItem()
    {
        this.WhenAnyValue(x => x.MasterVolume)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(VolumeChangeTimer);
    }

    public PresetItem(string name) : this()
    {
        Name = name;
    }

    private void VolumeChangeTimer(double value)
    {
        foreach (var item in Files)
        {
            item.AdjustVolume(value);
        }
    }

    public double MasterVolume
    {
        get => _masterVolume;
        set => this.RaiseAndSetIfChanged(ref _masterVolume, value);
    }
    private double _masterVolume = 100;

    /// <summary>
    /// Copies this preset into specified preset object.
    /// </summary>
    public void SaveAs(PresetItem dst)
    {
        dst.CheckNotNull(nameof(dst));

        dst.Name = Name;
        dst.MasterVolume = MasterVolume;
        dst.Files.Clear();
        // TODO: must clone; but it's breaking master volume 
        dst.Files.AddRange(Files.Select(x => x));
    }
}