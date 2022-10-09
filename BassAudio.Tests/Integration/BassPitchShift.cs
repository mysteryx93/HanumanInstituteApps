using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using ManagedBass;
using ManagedBass.Enc;
using ManagedBass.Fx;
using ManagedBass.Mix;
using Xunit.Abstractions;
using Bass = ManagedBass.Bass;
using BassEnc = ManagedBass.Enc.BassEnc;

namespace HanumanInstitute.BassAudio.Tests.Integration;

public class BassPitchShift : TestsBase
{
    public BassPitchShift(ITestOutputHelper output) : base(output)
    {
    }

    [Theory]
    [InlineData(.8)]
    [InlineData(.9)]
    [InlineData(432.0 / 440)]
    [InlineData(1.2)]
    public void PitchShift(double pitch)
    {
        var roundPitch = true;
        var source = "SourceShort.mp3";
        var destination = "SourceShort_out.mp3";
        var speed = 1.0;
        var rate = 1.0;

        var v = BassEnc.Version;
        // Create channel.
        Bass.Init();
        var chan = Bass.CreateStream(source, Flags: BassFlags.Float | BassFlags.Decode);
        var srcDuration = GetDuration(chan);
        var chanInfo = Bass.ChannelGetInfo(chan);
        var length = Bass.ChannelGetLength(chan);

        // Add mix effect.
        var sampleRate = 48000;
        var chanMix = BassMix.CreateMixerStream(sampleRate, chanInfo.Channels, BassFlags.MixerEnd | BassFlags.Decode).Valid();
        BassMix.MixerAddChannel(chanMix, chan, BassFlags.MixerChanNoRampin | BassFlags.AutoFree);

        // Add tempo effects.
        var chanOut = BassFx.TempoCreate(chanMix, BassFlags.Decode).Valid();
        //Bass.ChannelSetAttribute(chan, ChannelAttribute.TempoUseAAFilter, settings.AntiAlias ? 1 : 0);
        //Bass.ChannelSetAttribute(chan, ChannelAttribute.TempoAAFilterLength, settings.AntiAliasLength);

        // Optimized pitch shifting for increased quality
        // 1. Rate shift to Output * Pitch (rounded)
        // 2. Resample to Output (48000hz)
        // 3. Tempo adjustment: -Pitch
        var r = pitch;
        if (roundPitch)
        {
            r = Fraction.RoundToFraction(r);
        }
        var t = r;

        // 1. Rate Shift (lossless)
        Bass.ChannelSetAttribute(chanOut, ChannelAttribute.Frequency, sampleRate * r);
        // 2. Resampling to output in _chanMix constructor
        // 3. Tempo adjustment
        Bass.ChannelSetAttribute(chanOut, ChannelAttribute.Tempo,
            (1.0 / t - 1.0) * 100.0);

        // Create encoder.
        // BassEnc.EncodeStart(chanMix, destination, EncodeFlags.PCM | EncodeFlags.Dither | EncodeFlags.AutoFree, null);
        BassEnc_Mp3.Start(chanMix, null, EncodeFlags.Dither | EncodeFlags.AutoFree, destination);

        // Process file.
        var buffer = new byte[32 * 1024];
        while (true)
        {
            // Reading data moves the encoder forward, no need to use the buffer ourselves.
            var read = Bass.ChannelGetData(chanMix, buffer, buffer.Length);
            if (read == -1) { break; }
        }

        Bass.StreamFree(chanMix);
        Bass.StreamFree(chan);

        chan = Bass.CreateStream(destination, Flags: BassFlags.Float | BassFlags.Decode);
        var dstDuration = GetDuration(chan);
        Output.WriteLine($"Destination duration: {dstDuration.TotalSeconds:F3} seconds");
        var ratio = dstDuration / srcDuration;
        Output.WriteLine($"Ratio: {ratio:P2}");
        var rr = (1 - (1 / ratio)) / (1 - pitch);
        Output.WriteLine($"Slowdown / Pitch: {rr:P2}");

        Bass.Free();
    }

    [Fact]
    public void WavEncoder()
    {
        var source = "SourceShort.mp3";
        var destination = "SourceShort_out.wav";

        Bass.Init(0);
        var chan = Bass.CreateStream(source, Flags: BassFlags.Decode);
        BassEnc.EncodeStart(chan, CommandLine: destination, EncodeFlags.PCM | EncodeFlags.AutoFree, null);

        var buffer = new byte[16 * 1024];
        while (Bass.ChannelGetData(chan, buffer, buffer.Length) > -1)
        {
        }

        Bass.StreamFree(chan);
    }

    private TimeSpan GetDuration(int chan) =>
        TimeSpan.FromSeconds(ManagedBass.Bass.ChannelBytes2Seconds(chan, ManagedBass.Bass.ChannelGetLength(chan)));
}
