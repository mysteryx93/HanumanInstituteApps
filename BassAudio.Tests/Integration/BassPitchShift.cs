using ManagedBass;
using ManagedBass.Enc;
using ManagedBass.Fx;
using ManagedBass.Mix;
using Xunit.Abstractions;
using Bass = ManagedBass.Bass;
using BassEnc = ManagedBass.Enc.BassEnc;

namespace HanumanInstitute.BassAudio.Tests.Integration;

public class BassPitchShift
{
    public BassPitchShift(ITestOutputHelper output)
    {
        _output = output;
    }

    private readonly ITestOutputHelper _output;

    [Theory]
    [InlineData(.8)]
    [InlineData(.9)]
    [InlineData(432.0 / 440)]
    [InlineData(1.2)]
    public void PitchShift(double pitch)
    {
        var source = "SourceShort.mp3";
        var destination = "SourceShort_out.mp3";
        var speed = 1.0;
        var rate = 1.0;

        var v = BassEnc.Version;
        // Create channel.
        Bass.Init();
        var chan = Bass.CreateStream(source, Flags: BassFlags.Float | BassFlags.Decode);
        var chanInfo = Bass.ChannelGetInfo(chan);
        var srcDuration = GetDuration(chan);
        _output.WriteLine($"Pitch: {pitch}");
        _output.WriteLine($"Source duration: {srcDuration.TotalSeconds:F3} seconds");

        // Add tempo effects.
        chan = BassFx.TempoCreate(chan, BassFlags.Decode).Valid();
        Bass.Configure(Configuration.SRCQuality, 4);

        // In BASS, 2x speed is 100 (+100%), whereas our Speed property is 2. Need to convert.
        // speed 1=0, 2=100, 3=200, 4=300, .5=-100, .25=-300
        Bass.ChannelSetAttribute(chan, ChannelAttribute.Tempo, (1.0 / pitch * speed - 1.0) * 100.0);
        Bass.ChannelSetAttribute(chan, ChannelAttribute.TempoFrequency, chanInfo.Frequency * pitch * rate);

        // Add mix effect.
        var chanMix = BassMix.CreateMixerStream(44100, chanInfo.Channels, BassFlags.MixerEnd | BassFlags.Decode).Valid();
        BassMix.MixerAddChannel(chanMix, chan, BassFlags.MixerChanNoRampin | BassFlags.AutoFree);

        // Create encoder.
        // BassEnc.EncodeStart(chanMix, destination, EncodeFlags.PCM | EncodeFlags.Dither | EncodeFlags.AutoFree, null);
        BassEnc_Mp3.Start(chanMix, null, EncodeFlags.Dither | EncodeFlags.AutoFree, destination);
        var length = Bass.ChannelGetLength(chan, PositionFlags.Bytes);

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
        _output.WriteLine($"Destination duration: {dstDuration.TotalSeconds:F3} seconds");
        var ratio = dstDuration / srcDuration;
        _output.WriteLine($"Ratio: {ratio:P2}");
        var r = (1 - (1 / ratio)) / (1 - pitch);
        _output.WriteLine($"Slowdown / Pitch: {r:P2}");

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
