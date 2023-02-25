using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using LazyCache;
using ManagedBass;
using ReactiveUI;
using Xunit.Abstractions;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.BassAudio.Tests.Integration;

public class AudioEncoderTests : TestsBase
{
    public AudioEncoderTests(ITestOutputHelper output) : base(output)
    {
    }

    public IAudioEncoder Model => _model ??= SetupModel();
    private IAudioEncoder _model;
    private IAudioEncoder SetupModel()
    {
        BassDevice.Instance.Init(0);
        return new AudioEncoder(new PitchDetector(FileSystem, AppCache), FileSystem);
    }

    public EncodeSettings Settings { get; set; } = new EncodeSettings() { QualityOrSpeed = 2 };

    public IAppCache AppCache => _appCache ??= new CachingService();
    private IAppCache _appCache;

    public IFileSystemService FileSystem => _fileSystem ??= new FileSystemService(new FileSystem(), new WindowsApiService());
    private IFileSystemService _fileSystem;

    public ProcessingItem CreateSourceShort(EncodeFormat dstFormat = EncodeFormat.Mp3, EncodeFormat srcFormat = EncodeFormat.Mp3) =>
        CreateSource("SourceShort", dstFormat, srcFormat);
    public ProcessingItem CreateSourceLong(EncodeFormat dstFormat = EncodeFormat.Mp3, EncodeFormat srcFormat = EncodeFormat.Mp3) =>
        CreateSource("SourceLong", dstFormat, srcFormat);
    public ProcessingItem CreateSource(string source, EncodeFormat dstFormat = EncodeFormat.Mp3, EncodeFormat srcFormat = EncodeFormat.Mp3)
    {
        var srcExt = GetFormatExtension(srcFormat);
        var dstExt = GetFormatExtension(dstFormat);
        return new ProcessingItem($"{source}.{srcExt}", $"{source}.{srcExt}") { Destination = $"{source}_out.{dstExt}" };
    }

    private static string GetFormatExtension(EncodeFormat format) => format switch
    {
        EncodeFormat.Mp3 => "mp3",
        EncodeFormat.Aac => "m4a",
        EncodeFormat.Flac => "flac",
        EncodeFormat.Ogg => "ogg",
        EncodeFormat.Opus => "opus",
        EncodeFormat.Wav => "wav",
        _ => ""
    };

    public string GetTag(string propertyName, string file = "SourceShort.mp3") =>
        GetTag(x => typeof(TagsReader).GetProperty(propertyName)!.GetValue(x) as string);

    public string GetTag(Func<TagsReader, string> property, string file = "SourceShort.mp3")
    {
        BassDevice.Instance.Init(0);
        var chan = Bass.CreateStream(file, Flags: BassFlags.Decode).Valid();
        // var chanInfo = Bass.ChannelGetInfo(chan);
        var tags = new TagsReader(chan);
        var result = property(tags);
        Bass.StreamFree(chan);
        return result;
    }

    public static IEnumerable<object[]> GetAllSampleRates()
    {
        foreach (var format in Enum.GetValues<EncodeFormat>())
        {
            foreach (var sampleRate in new[]
                     {
                         0, 8000, 11025, 16000, 22050, 24000, 32000, 44100, 48000, 88200, 96000, 176400, 192000, 352800, 384000
                     })
            {
                yield return new object[] { format, sampleRate };
            }
        }
    }

    [Fact]
    public async Task Start_NoPath_ThrowsArgumentException()
    {
        var file = new ProcessingItem("", "") { Destination = "Dest1.mp3" };

        var t1 = Model.StartAsync(file, Settings);

        await Assert.ThrowsAsync<ArgumentException>(() => t1);
    }

    [Fact]
    public async Task Start_NoDestination_ThrowsArgumentException()
    {
        var file = new ProcessingItem("Source", "Source");

        var t1 = Model.StartAsync(file, Settings);

        await Assert.ThrowsAsync<ArgumentException>(() => t1);
    }

    [Fact]
    public async Task Start_FileNotFound_ThrowsFileNotFoundException()
    {
        var file = CreateSource("InvalidSource");

        var t1 = Model.StartAsync(file, Settings);

        await Assert.ThrowsAsync<FileNotFoundException>(() => t1);
    }

    [Fact]
    public async Task Start_InvalidSourcePath_ThrowsFileNotFoundException()
    {
        var file = CreateSource("/*\\");

        var t1 = Model.StartAsync(file, Settings);

        await Assert.ThrowsAsync<FileNotFoundException>(() => t1);
    }

    [Fact]
    public async Task Start_SourceFileNotAudio_ThrowsBassException()
    {
        var file = new ProcessingItem("BassAudio.dll", "BassAudio.dll") { Destination = "_out" };

        var t1 = Model.StartAsync(file, Settings);

        await Assert.ThrowsAsync<BassException>(() => t1);
    }

    [Fact]
    public async Task Start_SettingsOutOfRange_ThrowsValidationException()
    {
        var file = CreateSourceShort();
        Settings.Speed = 0;

        var t1 = Model.StartAsync(file, Settings);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => t1);
        Output.WriteLine(ex.Message);
    }

    [Theory]
    [InlineData(EncodeFormat.Mp3)]
    [InlineData(EncodeFormat.Wav)]
    [InlineData(EncodeFormat.Flac)]
    [InlineData(EncodeFormat.Opus)]
    [InlineData(EncodeFormat.Ogg)]
    public async Task Start_ShortFile_CreateOutputFile(EncodeFormat format)
    {
        var file = CreateSourceShort(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;
        file.WhenAnyValue(x => x.ProgressPercent).Subscribe(x =>
        {
            Output.WriteLine(x.ToStringInvariant());
        });

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        Assert.True(FileSystem.FileInfo.New(file.Destination).Length > 0);
    }

    [Theory]
    [InlineData(EncodeFormat.Mp3)]
    [InlineData(EncodeFormat.Wav)]
    [InlineData(EncodeFormat.Flac)]
    [InlineData(EncodeFormat.Opus)]
    [InlineData(EncodeFormat.Ogg)]
    public async Task Start_LongFile_CreateOutputFile(EncodeFormat format)
    {
        var file = CreateSourceLong(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;
        file.WhenAnyValue(x => x.ProgressPercent).Subscribe(x =>
        {
            Output.WriteLine(x.ToStringInvariant());
        });

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        Assert.True(FileSystem.FileInfo.New(file.Destination).Length > 0);
        Assert.Equal(1.0, file.ProgressPercent);
    }

    [Theory]
    [InlineData(EncodeFormat.Mp3, 0)]
    [InlineData(EncodeFormat.Mp3, 32)]
    [InlineData(EncodeFormat.Mp3, 128)]
    [InlineData(EncodeFormat.Mp3, 256)]
    [InlineData(EncodeFormat.Ogg, 0)]
    [InlineData(EncodeFormat.Ogg, 128)]
    [InlineData(EncodeFormat.Ogg, 192)]
    [InlineData(EncodeFormat.Ogg, 256)]
    [InlineData(EncodeFormat.Opus, 0)]
    [InlineData(EncodeFormat.Opus, 32)]
    [InlineData(EncodeFormat.Opus, 128)]
    [InlineData(EncodeFormat.Opus, 256)]
    public async Task Start_Bitrate_CreateFileOfSize(EncodeFormat format, int bitrate)
    {
        var file = CreateSourceShort(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;
        Settings.Bitrate = bitrate;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        var fileLength = FileSystem.FileInfo.New(file.Destination).Length;
        Output.WriteLine(fileLength.ToString());
    }

    [Theory]
    [MemberData(nameof(GetAllSampleRates))]
    public async Task Start_SampleRate_CreateFileWithSampleRate(EncodeFormat format, int sampleRate)
    {
        var sourceSampleRate = 48000;
        var file = CreateSourceShort(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;
        Settings.Bitrate = 128;
        Settings.SampleRate = sampleRate;
        Settings.PitchTo = 432;
        var shouldSucceed = sampleRate == 0 || Model.GetSupportedSampleRates(format).Contains(sampleRate);
        var threwError = false;

        try
        {
            await Model.StartAsync(file, Settings);
        }
        catch
        {
            threwError = true;
        }

        if (shouldSucceed)
        {
            Output.WriteLine("Expecting success");
            Assert.True(FileSystem.File.Exists(file.Destination));
            var chan = Bass.CreateStream(file.Destination);
            var chanInfo = Bass.ChannelGetInfo(chan);
            Bass.StreamFree(chan);
            Output.WriteLine(chanInfo.Frequency.ToString());
            Assert.False(threwError);
            Assert.Equal(sampleRate > 0 ? sampleRate : sourceSampleRate, chanInfo.Frequency);
        }
        else
        {
            Output.WriteLine("Unsupported sample rate, expecting failure");
            if (FileSystem.File.Exists(file.Destination))
            {
                var chan = Bass.CreateStream(file.Destination);
                var chanInfo = Bass.ChannelGetInfo(chan);
                Bass.StreamFree(chan);
                Output.WriteLine(chanInfo.Frequency.ToString());
                Assert.NotEqual(sampleRate > 0 ? sampleRate : sourceSampleRate, chanInfo.Frequency);
            }
            else
            {
                Output.WriteLine("Threw exception");
            }
        }
    }

    [Theory]
    [InlineData(EncodeFormat.Wav, 0)]
    [InlineData(EncodeFormat.Wav, 8)]
    [InlineData(EncodeFormat.Wav, 16)]
    [InlineData(EncodeFormat.Wav, 24)]
    [InlineData(EncodeFormat.Wav, 32)]
    [InlineData(EncodeFormat.Flac, 0)]
    [InlineData(EncodeFormat.Flac, 8)]
    [InlineData(EncodeFormat.Flac, 16)]
    [InlineData(EncodeFormat.Flac, 24)]
    [InlineData(EncodeFormat.Flac, 32)]
    public async Task Start_Bits_CreateFileWithBits(EncodeFormat format, int bits)
    {
        var file = CreateSourceShort(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;
        Settings.Bitrate = 128;
        Settings.BitsPerSample = bits;
        Settings.PitchTo = 432;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        var chan = Bass.CreateStream(file.Destination);
        var chanInfo = Bass.ChannelGetInfo(chan);
        Bass.StreamFree(chan);
        Output.WriteLine(chanInfo.OriginalResolution.ToString());
        Assert.Equal(bits > 0 ? bits : 16, chanInfo.OriginalResolution);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(8)]
    [InlineData(32)]
    [InlineData(128)]
    public async Task Start_AntiAlias_CreateFile(int? antiAliasLength)
    {
        var file = CreateSourceShort(EncodeFormat.Opus);
        file.Destination = $"_AntiAlias-{antiAliasLength}.opus";
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = EncodeFormat.Opus;
        Settings.Bitrate = 256;
        Settings.PitchTo = 432;
        Settings.AntiAlias = antiAliasLength.HasValue;
        Settings.AntiAliasLength = antiAliasLength ?? 32;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        // Listen to file to see difference
    }

    [Theory]
    [InlineData(.5)]
    [InlineData(1)]
    [InlineData(1.5)]
    public async Task Start_Speed_CreateFile(double speed)
    {
        var file = CreateSourceShort(EncodeFormat.Opus);
        file.Destination = $"_Speed-{speed}.opus";
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = EncodeFormat.Opus;
        Settings.Bitrate = 256;
        Settings.PitchTo = 432;
        Settings.Speed = speed;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        // Listen to file to see difference
    }

    [Theory]
    [InlineData(.5)]
    [InlineData(1)]
    [InlineData(1.5)]
    public async Task Start_Rate_CreateFile(double rate)
    {
        var file = CreateSourceShort(EncodeFormat.Opus);
        file.Destination = $"_Rate-{rate}.opus";
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = EncodeFormat.Opus;
        Settings.Bitrate = 256;
        Settings.PitchTo = 432;
        Settings.Rate = rate;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        // Listen to file to see difference
    }

    [Theory]
    [InlineData(1)]
    [InlineData(420)]
    [InlineData(432)]
    [InlineData(528)]
    [InlineData(10000)]
    public async Task Start_Pitch_CreateFile(double pitch)
    {
        var file = CreateSourceShort(EncodeFormat.Opus);
        file.Destination = $"_Pitch-{pitch}.opus";
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = EncodeFormat.Opus;
        Settings.Bitrate = 256;
        Settings.PitchTo = pitch;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        // Listen to file to see difference
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Start_Mp3QualitySpeed_CompareTimes(int quality)
    {
        var file = CreateSourceShort(EncodeFormat.Mp3);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = EncodeFormat.Mp3;
        Settings.Bitrate = 256;
        Settings.QualityOrSpeed = quality;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        // Look at test run times
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Start_FlacCompression_CompareTimes(int compression)
    {
        var file = CreateSourceShort(EncodeFormat.Flac);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = EncodeFormat.Flac;
        Settings.QualityOrSpeed = compression;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        var fileSize = FileSystem.FileInfo.New(file.Destination).Length;
        Output.WriteLine(fileSize.ToString());
        // Look at test run times
    }

    [Theory]
    [InlineData(EncodeFormat.Aac, "Title")]
    [InlineData(EncodeFormat.Aac, "Artist")]
    [InlineData(EncodeFormat.Aac, "Album")]
    [InlineData(EncodeFormat.Aac, "Year")]
    [InlineData(EncodeFormat.Aac, "Comment")]
    [InlineData(EncodeFormat.Aac, "Track")]
    [InlineData(EncodeFormat.Aac, "Genre")]
    [InlineData(EncodeFormat.Wav, "Title")]
    [InlineData(EncodeFormat.Wav, "Artist")]
    [InlineData(EncodeFormat.Wav, "Album")]
    [InlineData(EncodeFormat.Wav, "Year")]
    [InlineData(EncodeFormat.Wav, "Comment")]
    [InlineData(EncodeFormat.Wav, "Track")]
    [InlineData(EncodeFormat.Wav, "Genre")]
    [InlineData(EncodeFormat.Mp3, "Title")]
    [InlineData(EncodeFormat.Mp3, "Artist")]
    [InlineData(EncodeFormat.Mp3, "Album")]
    [InlineData(EncodeFormat.Mp3, "Year")]
    [InlineData(EncodeFormat.Mp3, "Comment")]
    [InlineData(EncodeFormat.Mp3, "Track")]
    [InlineData(EncodeFormat.Mp3, "Genre")]
    [InlineData(EncodeFormat.Flac, "Title")]
    [InlineData(EncodeFormat.Flac, "Artist")]
    [InlineData(EncodeFormat.Flac, "Album")]
    [InlineData(EncodeFormat.Flac, "Year")]
    [InlineData(EncodeFormat.Flac, "Comment")]
    [InlineData(EncodeFormat.Flac, "Track")]
    [InlineData(EncodeFormat.Flac, "Genre")]
    [InlineData(EncodeFormat.Opus, "Title")]
    [InlineData(EncodeFormat.Opus, "Artist")]
    [InlineData(EncodeFormat.Opus, "Album")]
    [InlineData(EncodeFormat.Opus, "Year")]
    [InlineData(EncodeFormat.Opus, "Comment")]
    [InlineData(EncodeFormat.Opus, "Track")]
    [InlineData(EncodeFormat.Opus, "Genre")]
    [InlineData(EncodeFormat.Ogg, "Title")]
    [InlineData(EncodeFormat.Ogg, "Artist")]
    [InlineData(EncodeFormat.Ogg, "Album")]
    [InlineData(EncodeFormat.Ogg, "Year")]
    [InlineData(EncodeFormat.Ogg, "Comment")]
    [InlineData(EncodeFormat.Ogg, "Track")]
    [InlineData(EncodeFormat.Ogg, "Genre")]
    public async Task Start_WithTag_SameTagInOutputFile(EncodeFormat format, string tagName)
    {
        var srcTag = GetTag(tagName);
        Output.WriteLine($"{tagName} = {srcTag}");
        var file = CreateSourceShort(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        var dstTag = GetTag(tagName, file.Destination);
        Output.WriteLine($"Output = {dstTag}");
        Assert.Equal(srcTag, dstTag);
    }

    [Fact]
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    public async Task Start_RoundPitch_CreateDifferentOutput()
    {
        var file1 = CreateSourceShort(EncodeFormat.Wav);
        var file2 = CreateSourceShort(EncodeFormat.Wav);
        file1.Destination = file1.Destination.Replace("out", "out1");
        file2.Destination = file1.Destination.Replace("out", "out2");
        FileSystem.DeleteFileSilent(file1.Destination);
        FileSystem.DeleteFileSilent(file2.Destination);
        Settings.Format = EncodeFormat.Wav;
        Settings.RoundPitch = false;

        await Model.StartAsync(file1, Settings);
        Settings.RoundPitch = true;
        await Model.StartAsync(file2, Settings);

        Assert.False(File.ReadAllBytes(file1.Destination).SequenceEqual(File.ReadAllBytes(file2.Destination)));
    }

    [Theory]
    [InlineData(0)] // Pitch
    [InlineData(1)] // Speed
    // [InlineData(2)] // Rate
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    public async Task Start_SkipTempo_CreateDifferentDuration(int test)
    {
        var file1 = CreateSourceShort(EncodeFormat.Wav);
        var file2 = CreateSourceShort(EncodeFormat.Wav);
        file1.Destination = file1.Destination.Replace("out", "out1");
        file2.Destination = file1.Destination.Replace("out", "out2");
        FileSystem.DeleteFileSilent(file1.Destination);
        FileSystem.DeleteFileSilent(file2.Destination);
        Settings.Format = EncodeFormat.Wav;
        Settings.AutoDetectPitch = false;
        if (test == 0)
        {
            Settings.PitchTo = 220;
        }
        else if (test == 1)
        {
            Settings.Speed = 1.1;
        }
        Settings.SkipTempo = false;

        await Model.StartAsync(file1, Settings);
        Settings.SkipTempo = true;
        await Model.StartAsync(file2, Settings);

        var chan = Bass.CreateStream(file1.Destination);
        var length1 = Bass.ChannelGetLength(chan);
        Bass.StreamFree(chan);
        chan = Bass.CreateStream(file2.Destination);
        var length2 = Bass.ChannelGetLength(chan);
        Bass.StreamFree(chan);
        Output.WriteLine(length1.ToStringInvariant());
        Output.WriteLine(length2.ToStringInvariant());
        Assert.NotEqual(length1, length2);
    }

    [Fact]
    public async Task Start_ShortFile_OutputFileNotLocked()
    {
        var file = CreateSourceShort();
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = EncodeFormat.Mp3;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        FileSystem.File.Delete(file.Destination);
    }

    [Fact]
    public async Task Start_SourceWavBitrateSource_OutputBitrate320()
    {
        var file = CreateSourceShort(EncodeFormat.Mp3, EncodeFormat.Wav);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Bitrate = 0;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        var fileLength = FileSystem.FileInfo.New(file.Destination).Length;
        Output.WriteLine(fileLength.ToString());
    }
}
