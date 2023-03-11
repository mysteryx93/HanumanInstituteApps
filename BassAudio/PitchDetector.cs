using LazyCache;
using ManagedBass;
using ManagedBass.Mix;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.BassAudio;

/// <inheritdoc />
public class PitchDetector : IPitchDetector
{
    private readonly IFileSystemService _fileSystem;
    private readonly IAppCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromDays(1);
    private const string CachePrefix = "BassAudio.PitchDetector: ";

    /// <inheritdoc />
    public ICollection<int> AnalyzeSampleRates { get; set; } = new List<int> { 44100, 32000, 24000 };

    /// <summary>
    /// Initializes a new instance of the PitchDetector class.
    /// </summary>
    public PitchDetector(IFileSystemService fileSystem, IAppCache cache)
    {
        _fileSystem = fileSystem;
        _cache = cache;
    }

    /// <inheritdoc />
    public Task<float> GetPitchAsync(string filePath, bool useCache = true) => useCache ?
        _cache.GetOrAddAsync(CachePrefix + filePath,
            () => GetPitchInternalAsync(filePath),
            _cacheExpiration) :
        Task.Run(() => GetPitchInternalAsync(filePath));

    /// <inheritdoc />
    public float GetPitch(string filePath, bool useCache = true) => useCache ? 
            _cache.GetOrAdd(CachePrefix + filePath, 
            () => GetPitchInternal(filePath),
            _cacheExpiration) :
            GetPitchInternal(filePath);

    private async Task<float> GetPitchInternalAsync(string filePath)
    {
        var results = await AnalyzeSampleRates.ForEachOrderedAsync(x => Task.Run(() => GetPitchInternal(filePath, x)));
        return results.MinBy(x => x.Sum).Frequency;
    }

    private float GetPitchInternal(string filePath)
    {
        var results = AnalyzeSampleRates.Select(x => GetPitchInternal(filePath, x));
        return results.MinBy(x => x.Sum).Frequency;
    }
    
    private FreqPeak GetPitchInternal(string filePath, int sampleRate)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException("Source audio file was not found.", filePath);
        }

        var toneFreq = ToneFreq;
        BassDevice.Instance.Init();

        // Get file stream.
        var chan = 0;
        var chanMix = 0;
        try
        {
            chan = Bass.CreateStream(filePath, Flags: BassFlags.Float | BassFlags.Decode).Valid();
            var chanInfo = Bass.ChannelGetInfo(chan);
            
            // Add mixer to change frequency.
            chanMix = BassMix.CreateMixerStream(sampleRate, chanInfo.Channels, BassFlags.MixerEnd | BassFlags.Decode).Valid();
            BassMix.MixerAddChannel(chanMix, chan, BassFlags.MixerChanNoRampin | BassFlags.AutoFree);

            var fft = new float[32768 / 2];
            var fftBuffer = new float[fft.Length];
            var freqStep = (float)sampleRate / fft.Length;

            // Read file and sum FFT values for up to 100 seconds.
            int read;
            var readTotal = 0;
            var maxRead = (int)Bass.ChannelSeconds2Bytes(chanMix, 100);
            do
            {
                read = Bass.ChannelGetData(chanMix, fftBuffer, (int)DataFlags.FFT32768);
                if (read > 0)
                {
                    readTotal += read;
                    for (var i = 0; i < fft.Length; i++)
                    {
                        fft[i] += fftBuffer[i];
                    }
                }
            } while (read > 0 && readTotal < maxRead);

            // Find the tuning frequency
            var maxSum = 0.0f;
            FreqPeak peak = new(0, 440);
            for (var i = 424f; i <= 448f; i += 0.1f)
            {
                var tones = Array.ConvertAll(toneFreq, x => x * i / 440f);
                // First and last tone are used for previous/next reference.
                for (var tone = 0; tone < 12; tone++)
                {
                    var sum = 0.0f;
                    for (var octave = 0; octave < 5; octave++)
                    {
                        var j = tone + octave * 12 + 1;
                        // We get more consistent results with rounding down (int) than with Math.Round
                        // var index = (int)Math.Round(tones[j] / freqStep, 0);
                        var index = (int)(tones[j] / freqStep);
                        // FFT bands are larger at lower frequencies and smaller at higher frequencies, compensate for that.
                        var factor = (tones[j] - tones[j - 1]) / freqStep;
                        // Applying a parabolic curve to favor middle-tones is not improving the results.
                        // var curve = (float)-Math.Pow(j - 1 - 40, 2) / 2000 + 1;
                        sum += fft[index] * factor;
                    }
                    if (sum > maxSum)
                    {
                        maxSum = sum;
                        peak = new FreqPeak(sum, i);
                    }
                }
            }
            // Analysis at smaller sampling rates gives smaller sums. I do not understand why, nor how to compensate for that.
            // This equation compensates for that difference and the .2577 constant came by trial and error tests.
            var adjust = (44100f / sampleRate - 1) *.2577f + 1;   
            return new FreqPeak(peak.Sum * adjust, peak.Frequency);
        }
        finally
        {
            Bass.StreamFree(chanMix);
            Bass.StreamFree(chan);
        }
    }

    private static float[]? _toneFreq;

    /// <summary>
    /// Returns a cached array of tones 20 to 62 (5 octaves + a tone before and after).
    /// </summary>
    protected static float[] ToneFreq
    {
        get
        {
            if (_toneFreq == null)
            {
                _toneFreq = new float[62];
                for (var i = 0; i < 62; i++)
                {
                    _toneFreq[i] = (float)Math.Pow(2, (20 + i - 49) / 12.0) * 440;
                }
            }
            return _toneFreq;
        }
    }
    
    private struct FreqPeak
    {
        public FreqPeak(float sum, float frequency)
        {
            Sum = sum;
            Frequency = frequency;
        }
        
        public float Sum { get; }
        public float Frequency { get; }

        public override string ToString() => $"Frequency: {Frequency}, Sum: {Sum}";
    }
}
