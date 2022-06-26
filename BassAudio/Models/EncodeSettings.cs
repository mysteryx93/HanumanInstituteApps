using System.ComponentModel.DataAnnotations;
using ReactiveUI;

namespace HanumanInstitute.BassAudio;

/// <summary>
/// Contains audio encoding settings. 
/// </summary>
public class EncodeSettings : ReactiveObject
{
    /// <summary>
    /// Gets or sets the encoding format.
    /// </summary>
    [Reactive]
    public EncodeFormat Format { get; set; }
    
    /// <summary>
    /// Gets or sets the encoding bitrate. 
    /// </summary>
    [Reactive]
    public int Bitrate { get; set; }
    
    /// <summary>
    /// Gets or sets the encoding sample rate.
    /// </summary>
    [Reactive]
    public int SampleRate { get; set; } 
    
    /// <summary>
    /// Gets or sets whether to enable the Anti-Alias filter.
    /// </summary>
    [Reactive]
    public bool AntiAlias { get; set; }

    /// <summary>
    /// Gets or sets the Anti-Alias filter length. 
    /// </summary>
    [Range(minimum: 8, maximum: 128)]
    public int AntiAliasLength { get; set; } = 32;

    /// <summary>
    /// Gets or sets the speed multiplier. Default=1.
    /// </summary>
    [Reactive]
    [RangeClusive(min: 0, minInclusive: false)]
    public double Speed { get; set; } = 1;

    /// <summary>
    /// Gets or sets the rate multiplier. Default=1.
    /// </summary>
    [Reactive]
    [RangeClusive(min: 0, minInclusive: false)]
    public double Rate { get; set; } = 1;

    /// <summary>
    /// Gets the pitch multiplier based on PitchFrom and PitchTo.
    /// </summary>
    public double Pitch => PitchTo / PitchFrom;
    
    /// <summary>
    /// Gets or sets the pitch of the source audio. 
    /// </summary>
    [Range(1, 10000)]
    public double PitchFrom
    {
        get => _pitchFrom;
        set
        {
            this.RaiseAndSetIfChanged(ref _pitchFrom, value);
            this.RaisePropertyChanged(nameof(Pitch));
        }
    }
    private double _pitchFrom = 440;

    /// <summary>
    /// Gets or sets the pitch to shift to.
    /// </summary>
    [Range(1, 10000)]
    public double PitchTo
    {
        get => _pitchTo;
        set
        {
            this.RaiseAndSetIfChanged(ref _pitchTo, value);
            this.RaisePropertyChanged(nameof(Pitch));
        }
    }
    private double _pitchTo = 440;

    /// <summary>
    /// Gets or sets the maximum amount of simultaneous encodes.
    /// </summary>
    [Reactive]
    [Range(1, 64)]
    public int MaxThreads { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to focus on quality or speed. 0 = fastest, 4 = best quality, 2 = default.
    /// </summary>
    [Range(0, 5)]
    public int QualityOrSpeed { get; set; } = 5;

    /// <summary>
    /// Gets the MP3 algorithm quality/speed based on QualityOrSpeed value. 0 = slowest but best quality, 9 = fastest but worst quality.
    /// </summary>
    public int Mp3QualitySpeed => QualityOrSpeed switch
    {
        5 => 0, // 14.074 sec
        4 => 1, // 8.548 sec
        3 => 2, // 6.121 sec
        2 => 3, // 3.769 sec
        1 => 5, // 3.262 sec
        0 => 7, // 1.949 sec
        _ => 3
    };

    /// <summary>
    /// Gets the FLAC compression setting based on QualityOrSpeed value. 0 = fastest compression, 8 = best compression.
    /// </summary>
    public int FlacCompression => QualityOrSpeed switch
    {
        5 => 8, // 1.348 sec, 28236664 bytes
        4 => 7, // 1.005 sec, 28261527 bytes
        3 => 6, // 0.957 sec, 28787935 bytes
        2 => 5, // 0.754 sec, 28852638 bytes
        1 => 4, // 0.766 sec, 28884732 bytes
        0 => 3, // 0.659 sec, 29637331 bytes
        _ => 5
    };

    /// <summary>
    /// Returns a clone of the current class.
    /// </summary>
    public EncodeSettings Clone() => (EncodeSettings)this.MemberwiseClone();
}
