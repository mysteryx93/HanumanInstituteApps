using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Abstractions;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using ManagedBass;
using ReactiveUI;
using Xunit.Abstractions;
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.BassAudio.Tests.Integration;

public class AudioEncoderTests
{
    public AudioEncoderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private readonly ITestOutputHelper _output;

    public IAudioEncoder Model => _model ??= SetupModel();
    private IAudioEncoder _model;
    private IAudioEncoder SetupModel()
    {
        BassDevice.Instance.Init(0);
        return new AudioEncoder(FileSystem);
    }

    public EncodeSettings Settings { get; set; } = new EncodeSettings() { QualityOrSpeed = 2 };

    public IFileSystemService FileSystem => _fileSystem ??= new FileSystemService(new FileSystem(), new WindowsApiService());
    private IFileSystemService _fileSystem;

    public ProcessingItem CreateSourceShort(EncodeFormat format = EncodeFormat.Mp3) => CreateSource("SourceShort", format);
    public ProcessingItem CreateSourceLong(EncodeFormat format = EncodeFormat.Mp3) => CreateSource("SourceLong", format);
    public ProcessingItem CreateSource(string source, EncodeFormat format = EncodeFormat.Mp3)
    {
        var ext = format switch
        {
            EncodeFormat.Mp3 => "mp3",
            EncodeFormat.Flac => "flac",
            EncodeFormat.Ogg => "ogg",
            EncodeFormat.Opus => "opus",
            EncodeFormat.Wav => "wav",
            _ => ""
        };
        return new ProcessingItem($"{source}.mp3", $"{source}.mp3") { Destination = $"{source}_out.{ext}" };
    }

    public string GetTag(string propertyName, string file = "SourceShort.mp3") =>
        GetTag(x => typeof(TagsReader).GetProperty(propertyName)!.GetValue(x) as string);

    public string GetTag(Func<TagsReader, string> property, string file = "SourceShort.mp3")
    {
        BassDevice.Instance.Init(0);
        var chan = Bass.CreateStream(file, Flags: BassFlags.Decode).Valid();
        var chanInfo = Bass.ChannelGetInfo(chan);
        var tags = new TagsReader(chan);
        var result = property(tags);
        Bass.StreamFree(chan);
        return result;
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
        _output.WriteLine(ex.Message);
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
            _output.WriteLine(x.ToStringInvariant());
        });

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        Assert.True(FileSystem.FileInfo.FromFileName(file.Destination).Length > 0);
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
            _output.WriteLine(x.ToStringInvariant());
        });

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        Assert.True(FileSystem.FileInfo.FromFileName(file.Destination).Length > 0);
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
        var fileLength = FileSystem.FileInfo.FromFileName(file.Destination).Length;
        _output.WriteLine(fileLength.ToString());
    }

    [Theory]
    [InlineData(EncodeFormat.Mp3, 0)]
    [InlineData(EncodeFormat.Mp3, 44100)]
    [InlineData(EncodeFormat.Mp3, 48000)]
    [InlineData(EncodeFormat.Ogg, 0)]
    [InlineData(EncodeFormat.Ogg, 44100)]
    [InlineData(EncodeFormat.Ogg, 48000)]
    [InlineData(EncodeFormat.Opus, 0)]
    // [InlineData(EncodeFormat.Opus, 44100)] // Opus does not support 44100
    [InlineData(EncodeFormat.Opus, 48000)]
    public async Task Start_SampleRate_CreateFileWithSampleRate(EncodeFormat format, int sampleRate)
    {
        var sourceSampleRate = 48000;
        var file = CreateSourceShort(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;
        Settings.Bitrate = 128;
        Settings.SampleRate = sampleRate;
        Settings.PitchTo = 432;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        var chan = Bass.CreateStream(file.Destination);
        var chanInfo = Bass.ChannelGetInfo(chan);
        Bass.StreamFree(chan);
        Assert.Equal(sampleRate > 0 ? sampleRate : sourceSampleRate, chanInfo.Frequency);
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
    [InlineData(528.0 / 440)]
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
        var fileSize = FileSystem.FileInfo.FromFileName(file.Destination).Length;
        _output.WriteLine(fileSize.ToString());
        // Look at test run times
    }

    [Theory]
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
        _output.WriteLine($"{tagName} = {srcTag}");
        var file = CreateSourceShort(format);
        FileSystem.DeleteFileSilent(file.Destination);
        Settings.Format = format;

        await Model.StartAsync(file, Settings);

        Assert.True(FileSystem.File.Exists(file.Destination));
        var dstTag = GetTag(tagName, file.Destination);
        _output.WriteLine($"Output = {dstTag}");
        Assert.Equal(srcTag, dstTag);
    }
}
