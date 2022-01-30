using System;
using System.Xml.Serialization;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

public class PlayingItem : ReactiveObject
{
    [XmlIgnore]
    public Guid Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }
    private Guid _id = Guid.NewGuid();

    public string FullPath
    {
        get => _fullPath;
        set => this.RaiseAndSetIfChanged(ref _fullPath, value);
    }
    private string _fullPath = string.Empty;
        
    [XmlIgnore]
    public string FileName => System.IO.Path.GetFileName(FullPath);

    [XmlElement("Volume")]
    public double FullVolume
    {
        get => _fullVolume;
        set => this.RaiseAndSetIfChanged(ref _fullVolume, value);
    }
    private double _fullVolume = 100;
        
    private double MasterVolume { get; set; } = -1;
    private bool _adjustingVolume;

    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }
    private bool _isPlaying = true;

    public PlayingItem()
    { }

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
        if (Math.Abs(MasterVolume - newMasterVolume) > .001)
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

    public double Rate
    {
        get => _rate;
        set => this.RaiseAndSetIfChanged(ref _rate, value);
    }
    private double _rate;
        
    public PlayingItem Clone() => (PlayingItem)MemberwiseClone();
}
