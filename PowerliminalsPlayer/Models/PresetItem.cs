using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

public class PresetItem : ReactiveObject
{
    [Reactive]
    public string Name { get; set; } = string.Empty;

    [Reactive]
    [XmlElement("File")]
    public ObservableCollectionWithRange<PlayingItem> Files { get; set; } = new();

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

    [Reactive]
    public double MasterVolume { get; set; } = 100;

    /// <summary>
    /// Copies this preset into specified preset object.
    /// </summary>
    public void SaveAs(PresetItem dst)
    {
        dst.CheckNotNull(nameof(dst));

        dst.Name = Name;
        dst.MasterVolume = MasterVolume;
        dst.Files.Clear();
        dst.Files.AddRange(Files.Select(x => x.Clone(MasterVolume)));
    }
    //
    // public PresetItem Clone()
    // {
    //     var result = new PresetItem()
    //     {
    //         Name = Name,
    //         MasterVolume = MasterVolume
    //     };
    //     result.Files.AddRange(Files.Select(x => x.Clone()));
    //     return result;
    // }
}
