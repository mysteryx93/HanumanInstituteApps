using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using LazyCache;
using ManagedBass;
using Xunit.Abstractions;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.BassAudio.Tests.Integration;

public class PitchDetectorTests : TestsBase
{
    public PitchDetectorTests(ITestOutputHelper output) : base(output)
    {
    }

    public PitchDetectorWithDebug Model => _model ??= SetupModel();
    private PitchDetectorWithDebug _model;
    private PitchDetectorWithDebug SetupModel()
    {
        return new PitchDetectorWithDebug(FileSystem);
    }

    public IFileSystemService FileSystem => _fileSystem ??= new FileSystemService(new FileSystem(), new WindowsApiService());
    private IFileSystemService _fileSystem;

    public IAppCache AppCache => _appCache ??= new CachingService();
    private IAppCache _appCache;

    [Theory]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/INNA/Inna/", new[] { 441.00104f, 439.90097f, 439.22592f, 440.90103f, 439.54095f, 437.5675f, 439.70096f, 440.501f, 440.00098f, 440.31766f, 440.70102f, 440.651f, 440.361f, 440.451f, 440.70102f, 441.9011f, 440.451f, 440.25098f })]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/Symphony X - V The New Mythology Suite/", new[] { 441.10104f, 441.24106f, 440.651f, 440.8677f, 442.30112f, 441.9511f, 441.16772f, 441.22604f, 440.46768f, 441.30106f, 441.85107f, 440.67603f, 441.86777f })]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/DJ Project/DJ Project - Experience/", new[] { 439.20093f, 439.5295f, 439.53427f, 440.70102f, 440.626f })]
    [InlineData("/run/media/hanuman/Storage-ntfs/Music/Enigma/Enigma - Best of Enigma CD1/", new[] { 440.95105f, 438.93423f, 441.30106f, 439.9343f, 441.40106f, 445.36798f, 442.95117f, 440.151f, 441.23438f, 440.95105f, 440.501f, 441.33438f, 440.68674f, 441.20105f, 441.60107f, 447.48145f, 441.20105f, 441.10104f })]
    public async Task DetectPitchAsync(string dir, float[] bestFreq)
    {
        var results = new List<(int, float)>();
        var allRates = new List<int>();
        for (var i = 44000; i >= 26000; i -= 1000)
        {
            allRates.Add(i);
        }
        await DetectPitchInternalAsync(dir, bestFreq, new[] { 42000, 34000, 27000 }); return;

        await allRates.ForEachAsync(async sampleRate =>
        {
            var diffSum = await DetectPitchInternalAsync(dir, bestFreq, new[] { 42000, 34000, 27000, 39000, 31000, sampleRate });
            results.Add((sampleRate, diffSum));
        });
        Output.WriteLine("");
        Output.WriteLine("");
        foreach (var item in results.OrderBy(x => x.Item2))
        {
            Output.WriteLine($"Diff {item.Item1}hz: {item.Item2}");
        }
    }

    public async Task<float> DetectPitchInternalAsync(string dir, float[] bestFreq, IList<int> sampleRates)
    {
        Model.AnalyzeSampleRates = sampleRates;
        var diffBest = new List<float>();
        var iter = 0;
        var strAll = new StringBuilder();

        foreach (var file in Directory.GetFiles(dir, "*.mp3"))
        {
            // var file = "SourceLong.mp3";
            var pitch = await Model.GetPitchAsync(file);

            var bestItem = iter < bestFreq.Length ? (float?)bestFreq[iter] : null;
            var diffWithBest = bestItem.HasValue ? Math.Abs(pitch - bestItem.Value) : 0;
            Output.WriteLine(
                $"{Path.GetFileName(file)} @ {Model.LastWinningSampleRate}hz: {pitch.ToStringInvariant()}  {(diffWithBest > 1f ? $"*({bestItem})*" : "")}");
            diffBest.Add(diffWithBest);
            Assert.True(pitch is > 400 and < 450);
            strAll.Append((strAll.Length > 0 ? ", " : "") + pitch + "f");
            iter++;
        }

        Output.WriteLine("");
        var history = Model.GetDiffHistory();
        for (var i = 1; i < history.Length; i++)
        {
            Output.WriteLine($"AverageDiff{i + 1}: " + history[i].ToString("P"));
        }
        var diffSum = diffBest.Sum();
        Output.WriteLine("Total difference with best pitch detection: " + diffSum);
        Output.WriteLine(strAll.ToString());
        Output.WriteLine("");
        return diffSum;
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
