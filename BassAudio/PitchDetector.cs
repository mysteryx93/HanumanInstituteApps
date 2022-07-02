using ManagedBass;
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
    }

    /// <inheritdoc />
    public async Task<float> GetPitchAsync(string filePath) =>
        await Task.Run(() => GetPitch(filePath), default).ConfigureAwait(false);

    /// <inheritdoc />
    public float GetPitch(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException("Source audio file was not found.", filePath);
        }

        var toneFreq = ToneFreq;
        BassDevice.Instance.Init();

        // Get file stream.
        var chan = Bass.CreateStream(filePath, Flags: BassFlags.Float | BassFlags.Decode).Valid();
        try
        {
            var chanInfo = Bass.ChannelGetInfo(chan);
            var fft = new float[32768 / 2];
            var fftBuffer = new float[fft.Length];
            var freqStep = (float)chanInfo.Frequency / fft.Length;

            // Read file and sum FFT values for up to 100 seconds.
            int read;
            var readTotal = 0;
            var maxRead = (int)Bass.ChannelSeconds2Bytes(chan, 100);
            do
            {
                read = Bass.ChannelGetData(chan, fftBuffer, (int)DataFlags.FFT32768);
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
            var maxFreq = 440.0f;
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
                        maxFreq = i;
                    }
                }
            }
            return maxFreq;
        }
        finally
        {
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
}
