using HanumanInstitute.Common.Services.Validation;
using ManagedBass;
using ManagedBass.Enc;
using ManagedBass.Fx;
using ManagedBass.Mix;
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.BassAudio;

/// <inheritdoc />
public class AudioEncoder : IAudioEncoder
{
    private readonly IPitchDetector _pitchDetector;
    private readonly IFileSystemService _fileSystem;

    /// <summary>
    /// Initializes a new instance of the AudioEncoder class.
    /// </summary>
    public AudioEncoder(IPitchDetector pitchDetector, IFileSystemService fileSystem)
    {
        _pitchDetector = pitchDetector;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public async Task StartAsync(ProcessingItem file, EncodeSettings settings, CancellationToken cancellationToken = default) =>
        await Task.Run(() => Start(file, settings, cancellationToken), default).ConfigureAwait(false);

    /// <inheritdoc />
    public void Start(ProcessingItem file, EncodeSettings settings, CancellationToken cancellationToken = default)
    {
        file.Path.CheckNotNullOrEmpty(nameof(file.Path));
        file.Destination.CheckNotNullOrEmpty(nameof(file.Destination));
        settings.ValidateAndThrow();

        // Prepare.
        BassDevice.Instance.Init();
        if (!_fileSystem.File.Exists(file.Path))
        {
            throw new FileNotFoundException("Source audio file was not found.", file.Path);
        }
        _fileSystem.EnsureDirectoryExists(file.Destination);

        // Calculate pitch.
        if (settings.AutoDetectPitch)
        {
            file.Pitch ??= _pitchDetector.GetPitch(file.Path);
        }
        var pitch = settings.AutoDetectPitch ? 
            settings.PitchTo / file.Pitch!.Value : 
            settings.Pitch;
        
        // Create channel.
        var chan = Bass.CreateStream(file.Path, Flags: BassFlags.Float | BassFlags.Decode).Valid();
        file.Status = EncodeStatus.Processing;
        try
        {
            var tags = new TagsReader(chan);
            
            var chanInfo = Bass.ChannelGetInfo(chan);
            var bitrate = (int)Bass.ChannelGetAttribute(chan, ChannelAttribute.Bitrate);
            bitrate = BitrateRoundUp(bitrate);
            var length = Bass.ChannelGetLength(chan);

            // Add tempo effects.
            chan = BassFx.TempoCreate(chan, BassFlags.Decode).Valid();
            Bass.ChannelSetAttribute(chan, ChannelAttribute.TempoUseAAFilter, settings.AntiAlias ? 1 : 0);
            Bass.ChannelSetAttribute(chan, ChannelAttribute.TempoAAFilterLength, settings.AntiAliasLength);
            Bass.Configure(Configuration.SRCQuality, 4);

            // In BASS, 2x speed is 100 (+100%), whereas our Speed property is 2. Need to convert.
            // speed 1=0, 2=100, 3=200, 4=300, .5=-100, .25=-300
            Bass.ChannelSetAttribute(chan, ChannelAttribute.Tempo, (1.0 / pitch * settings.Speed - 1.0) * 100.0);
            Bass.ChannelSetAttribute(chan, ChannelAttribute.TempoFrequency,
                chanInfo.Frequency * pitch * settings.Rate);

            var speed = settings.Speed / pitch;
            var tempo = speed >= 1 ? -100.0 / speed + 100 : 100.0 * speed - 100;
            var freq = chanInfo.Frequency * settings.Rate * pitch;
            Bass.ChannelSetAttribute(chan, ChannelAttribute.Tempo, tempo);
            Bass.ChannelSetAttribute(chan, ChannelAttribute.TempoFrequency, freq);

            // Add mix effect.
            var sampleRate = settings.Format == EncodeFormat.Opus ? 48000 :
                settings.SampleRate > 0 ? settings.SampleRate : chanInfo.Frequency;
            var chanMix = BassMix.CreateMixerStream(sampleRate, chanInfo.Channels, BassFlags.MixerEnd | BassFlags.Decode).Valid();
            BassMix.MixerAddChannel(chanMix, chan, BassFlags.MixerChanNoRampin | BassFlags.AutoFree);
            // chan = chanMix;

            if (cancellationToken.IsCancellationRequested)
            {
                file.Status = EncodeStatus.Cancelled;
                return;
            }

            // Create encoder.
            var options = GetOptions(settings, bitrate, chanInfo, tags);
            var flags = EncodeFlags.ConvertFloatTo8BitInt | EncodeFlags.Dither | EncodeFlags.AutoFree;
            var encHandle = settings.Format switch
            {
                // CommandLine is necessary to switch overload https://github.com/ManagedBass/ManagedBass/issues/113
                EncodeFormat.Wav => BassEnc.EncodeStart(chanMix, CommandLine: file.Destination, EncodeFlags.PCM | flags, null).Valid(),
                EncodeFormat.Mp3 => BassEnc_Mp3.Start(chanMix, options, flags, file.Destination).Valid(),
                EncodeFormat.Flac => BassEnc_Flac.Start(chanMix, options, flags, file.Destination).Valid(),
                EncodeFormat.Ogg => BassEnc_Ogg.Start(chanMix, options, flags, file.Destination).Valid(),
                EncodeFormat.Opus => BassEnc_Opus.Start(chanMix, options, flags, file.Destination).Valid(),
                _ => throw new ArgumentException(@"Invalid encode format.", nameof(settings.Format))
            };

            // Process file.
            // Reading data moves the encoder forward, no need to use the buffer ourselves.
            var buffer = new byte[32 * 1024];
            while (Bass.ChannelGetData(chanMix, buffer, buffer.Length) > -1)
            {
                var pos = Bass.ChannelGetPosition(chan);
                file.ProgressPercent = Math.Round((double)pos / length, 3);

                if (BassEnc.EncodeIsActive(encHandle) == PlaybackState.Stopped)
                {
                    file.Status = EncodeStatus.Error;
                    return;
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    file.Status = EncodeStatus.Cancelled;
                    return;
                }
            }
            file.Status = EncodeStatus.Completed;
        }
        finally
        {
            Bass.StreamFree(chan);
        }
    }

    /// <summary>
    /// Creates a string of option arguments to send to the encoder. 
    /// </summary>
    /// <param name="settings">The encoding settings.</param>
    /// <param name="sourceBitrate">The bitrate of the source.</param>
    /// <param name="chanInfo">Information about the media channel.</param>
    /// <param name="tags">A class to read media tags.</param>
    private string GetOptions(EncodeSettings settings, int sourceBitrate, ChannelInfo chanInfo, TagsReader tags)
    {
        var bitrate = settings.Bitrate > 0 ? settings.Bitrate : sourceBitrate;
        // Limit MONO at 192kbps (OGG won't support it, and there's no reason to use higher). 
        if (chanInfo.Channels == 1 && bitrate > 192)
        {
            bitrate = 192;
        }

        return settings.Format switch
        {
            EncodeFormat.Mp3 => $"--abr {bitrate} -q {settings.Mp3QualitySpeed} --add-id3v2"
                .AddTag("--tt", tags.Title).AddTag("--ta", tags.Artist).AddTag("--tl", tags.Album).AddTag("--ty", tags.Year)
                .AddTag("--tc", tags.Comment).AddTag("--tn", tags.Track).AddTag("--tg", tags.Genre),
            EncodeFormat.Flac => $"--compression-level-{settings.FlacCompression}"
                .AddTag("-T title=", tags.Title).AddTag("-T artist=", tags.Artist).AddTag("-T album=", tags.Album).AddTag("--date", tags.Year)
                .AddTag("-T comment=", tags.Comment).AddTag("-T tracknum=", tags.Track).AddTag("-T genre=", tags.Genre),
            EncodeFormat.Ogg => $"--bitrate {bitrate}"
                .AddTag("--title", tags.Title).AddTag("--artist", tags.Artist).AddTag("--album", tags.Album).AddTag("--date", tags.Year)
                .AddTag("--comment", tags.Comment).AddTag("--tracknum", tags.Track).AddTag("--genre", tags.Genre),
            EncodeFormat.Opus => $"--bitrate {bitrate}"
                .AddTag("--title", tags.Title).AddTag("--artist", tags.Artist).AddTag("--album", tags.Album).AddTag("--date", tags.Year)
                .AddTag("--comment", tags.Comment).AddTag("--tracknumber", tags.Track).AddTag("--genre", tags.Genre),
            _ => ""
        };
    }

    /// <summary>
    /// Rounds up the bitrate to a standard bitrate.
    /// </summary>
    /// <param name="bitrate">The bitrate to round up.</param>
    /// <returns>A standard bitrate.</returns>
    private int BitrateRoundUp(int bitrate) =>
        bitrate switch
        {
            _ when bitrate <= 0 => 0,
            _ when bitrate <= 96 => 96,
            _ when bitrate <= 128 => 128,
            _ when bitrate <= 192 => 192,
            _ when bitrate <= 256 => 256,
            _ when bitrate <= 320 => 320,
            _ => bitrate
        };
}
