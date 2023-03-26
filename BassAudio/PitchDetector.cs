using ManagedBass;
using ManagedBass.Mix;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.BassAudio;

/// <inheritdoc />
public class PitchDetector : IPitchDetector
{
    private readonly IFileSystemService _fileSystem;

    /// <summary>
    /// Initializes a new instance of the PitchDetector class.
    /// </summary>
    public PitchDetector(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
        // ReSharper disable once VirtualMemberCallInConstructor
        PassesChanged();
    }

    /// <inheritdoc />
    public IEnumerable<int>? AnalyzeSampleRates
    {
        get => _analyzeSampleRates;
        set
        {
            _analyzeSampleRates = value;
            PassesChanged();
        }
    }
    private IEnumerable<int>? _analyzeSampleRates;

    /// <inheritdoc />
    public int AnalyzePasses
    {
        get => _analyzePasses;
        set
        {
            value.CheckRange(nameof(AnalyzePasses), min: 1, max: 3);
            _analyzePasses = value;
            PassesChanged();
        }
    }
    private int _analyzePasses = 3;

    /// <summary>
    /// Occurs after setting AnalyzeSampleRates or AnalyzePasses.
    /// </summary>
    protected virtual void PassesChanged()
    { }

    /// <inheritdoc />
    public virtual async Task<float> GetPitchAsync(string filePath)
    {
        var results = await GetAnalyzeSampleRates().ForEachOrderedAsync(x => Task.Run(() => GetPitchInternal(filePath, x)));
        return SelectBest(results);
    }

    /// <inheritdoc />
    public virtual float GetPitch(string filePath)
    {
        var results = GetAnalyzeSampleRates().Select(x => GetPitchInternal(filePath, x)).ToList();
        return SelectBest(results);
    }

    /// <summary>
    /// Returns the list of sample rates at which to run analysis. 
    /// </summary>
    private IEnumerable<int> GetAnalyzeSampleRates() => AnalyzeSampleRates ?? new[] { 42000, 34000, 27000 }.Take(AnalyzePasses);

    /// <summary>
    /// Returns average of all frequencies within 3% of max sum.
    /// </summary>
    protected virtual float SelectBest(IList<FreqPeak> values)
    {
        // 
        var max = values.MaxBy(x => x.Sum).Sum;
        return values.Where(x => x.Sum >= max * .97).Select(x => x.Frequency).Average();
    }

    private FreqPeak GetPitchInternal(string filePath, int sampleRate)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException("Source audio file was not found.", filePath);
        }

        var toneFreq = ToneFreq;
        BassDevice.Instance.InitPlugins();
        // Changing these configurations does not improve results. 
        // Bass.Configure(Configuration.SRCQuality, 4);
        // Bass.Configure(Configuration.FloatDSP, true);

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
            var adjust = (44100f / sampleRate - 1) * .2577f + 1;
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

    /// <summary>
    /// Holds information about detected frequencies.
    /// </summary>
    protected readonly struct FreqPeak
    {
        /// <summary>
        /// Initializes a new instance of the FreqPeak class.
        /// </summary>
        /// <param name="sum">The sum or 'confidence' of the detected frequency.</param>
        /// <param name="frequency">The detected frequency.</param>
        public FreqPeak(float sum, float frequency)
        {
            Sum = sum;
            Frequency = frequency;
        }

        /// <summary>
        /// The sum or 'confidence' of the detected frequency.
        /// </summary>
        public float Sum { get; }
        /// <summary>
        /// The detected frequency.
        /// </summary>
        public float Frequency { get; }

        /// <summary>
        /// Returns the values as a string representation. 
        /// </summary>
        public override string ToString() => $"Frequency: {Frequency}, Sum: {Sum}";
    }
}
