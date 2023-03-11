using System.IO;
using System.IO.Abstractions;
using System.Linq;
using LazyCache;
using ManagedBass;
using Xunit.Abstractions;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.BassAudio.Tests.Integration;

public class PitchDetectorTests : TestsBase
{
    public PitchDetectorTests(ITestOutputHelper output) : base(output)
    { }

    public PitchDetector Model => _model ??= SetupModel();
    private PitchDetector _model;
    private PitchDetector SetupModel()
    {
        return new PitchDetector(FileSystem, AppCache);
    }

    public IFileSystemService FileSystem => _fileSystem ??= new FileSystemService(new FileSystem(), new WindowsApiService());
    private IFileSystemService _fileSystem;
    
    public IAppCache AppCache => _appCache ??= new CachingService();
    private IAppCache _appCache;

    [Theory]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/INNA/Inna/")]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/Symphony X - V The New Mythology Suite/")]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/DJ Project/DJ Project - Experience/")]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/Enigma/Enigma - Best of Enigma CD1/")]
    public async Task DetectPitchAsync(string dir)
    {
        foreach (var file in Directory.GetFiles(dir, "*.mp3"))
        {
            // var file = "SourceLong.mp3";
            var pitch = await Model.GetPitchAsync(file);

            Output.WriteLine($"{Path.GetFileName(file)}: {pitch.ToStringInvariant()}");
            Assert.True(pitch is > 400 and < 450);
        }
    }

    [Fact]
    public void Detect()
    {
        // BassDevice.Instance.Init();
        Bass.Init();
        var filePath = "SourceLong2.mp3";

        var chan = Bass.CreateStream(filePath, Flags: BassFlags.Decode).Valid();
        var fft = new float[512];
        var read = Bass.ChannelGetData(chan, fft, (int)DataFlags.FFT1024 | 100000);

        foreach (var f in fft)
        {
            Output.WriteLine(f.ToStringInvariant());
        }
        // Assert.Contains(fft, x => x > 0);
    }
}
