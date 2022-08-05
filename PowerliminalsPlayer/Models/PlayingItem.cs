using System.Xml.Serialization;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

public class PlayingItem : ReactiveObject
{
    [Reactive]
    [XmlIgnore]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullPath
    {
        get => _fullPath;
        set => this.RaiseAndSetIfChanged(ref _fullPath, value);
    }
    private string _fullPath = string.Empty;

    [XmlIgnore]
    public string FileName => System.IO.Path.GetFileName(FullPath);

    [Reactive]
    [XmlElement("Volume")]
    public double FullVolume { get; set; } = 100;

    private double MasterVolume { get; set; } = -1;
    private bool _adjustingVolume;

    [Reactive]
    public bool IsPlaying { get; set; } = true;

    public PlayingItem()
    {
    }

    public PlayingItem(string path, double masterVolume = 1.0)
    {
        _fullPath = path;
        // this.MasterVolume = masterVolume;
        AdjustVolume(masterVolume);
    }

    [XmlIgnore]
    public double Volume
    {
        get => _volume;
        set
        {
            this.RaiseAndSetIfChanged(ref _volume, value);
            if (!_adjustingVolume)
            {
                FullVolume = MasterVolume > 0 ? value * 100 / MasterVolume : 0;
            }
        }
    }
    private double _volume = 1.0;

    public void AdjustVolume(double newMasterVolume)
    {
        if (newMasterVolume >= 0 && Math.Abs(MasterVolume - newMasterVolume) > .001)
        {
            _adjustingVolume = true;
            Volume = FullVolume * newMasterVolume / 100;
            MasterVolume = newMasterVolume;
            _adjustingVolume = false;
        }
    }

    public int Speed
    {
        get => _speed;
        set
        {
            this.RaiseAndSetIfChanged(ref _speed, value);
            var factor = value / 8.0;
            Rate = factor < 0 ? 1 / (1 - factor) : 1 * (1 + factor);
        }
    }
    private int _speed;

    [Reactive]
    [XmlIgnore]
    public double Rate { get; private set; }

    public PlayingItem Clone(double newMasterVolume)
    {
        //MemberwiseClone caused weird behaviors.
        //return (PlayingItem)this.MemberwiseClone();

        // ReSharper disable once UseObjectOrCollectionInitializer
        var result = new PlayingItem(FullPath, MasterVolume);
        result._adjustingVolume = true;

        result.FullVolume = FullVolume;
        result.Id = Id;
        result.IsPlaying = IsPlaying;
        result.Speed = Speed;
        result.Volume = Volume;

        result._adjustingVolume = false;
        return result;
    }
}
