using System.Linq;
using LazyCache;

namespace HanumanInstitute.BassAudio.Tests.Integration;

public class PitchDetectorWithDebug : PitchDetector
{
    public PitchDetectorWithDebug(IFileSystemService fileSystem) : base(fileSystem)
    {
    }
    
    private List<float>[] _diffHistory = Array.Empty<List<float>>();
    
    public int LastWinningSampleRate { get; private set; }

    protected override void PassesChanged()
    {
        base.PassesChanged();
        _diffHistory = new List<float>[AnalyzeSampleRates?.Count() ?? AnalyzePasses];
        for (var i = 0; i < _diffHistory.Length; i++)
        {
            _diffHistory[i] = new List<float>();
        }
    }
    
    public float[] GetDiffHistory() => _diffHistory.Select(x => x.Any() ? x.Average() : 0).ToArray();

    protected override float SelectBest(IList<FreqPeak> values)
    {
        StoreDiffHistory(values);
        return base.SelectBest(values);
    }
    
    private void StoreDiffHistory(IList<FreqPeak> values)
    {
        float? firstVal = null;
        var i = 0;
        var maxIndex = 0;
        var maxValue = 0f;
        foreach (var item in values)
        {
            firstVal ??= item.Sum;
            if (item.Sum > maxValue)
            {
                maxIndex = i;
                maxValue = item.Sum;
            }
            _diffHistory[i++].Add(item.Sum / firstVal.Value - 1);
            LastWinningSampleRate = AnalyzeSampleRates!.ElementAt(maxIndex);
        }
    }
}
