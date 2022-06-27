using ManagedBass;

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
            }
            while (read > 0 && readTotal < maxRead);

            // Generate 440hz tones. The first one is not used except as "previous tone" reference.
            var toneFreq = new float[61];
            for (var i = 0; i < 61; i++)
            {
                toneFreq[i] = GetFrequency(i + 20);
            }
            
            // Find the tuning frequency
            var maxSum = 0.0f;
            var maxFreq = 440.0f;
            for (var i = 424.0f; i < 448.1f; i += 0.1f)
            {
                var sum = 0.0f;
                var tones = Array.ConvertAll(toneFreq, x => x * i / 440f);
                for (var j = 1; j < tones.Length; j++)
                {
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
            return maxFreq;
        }
        finally
        {
            Bass.StreamFree(chan);
        }
    }

    private float GetFrequency(int keyIndex) => (float)Math.Pow(2, (keyIndex - 49) / 12.0) * 440;
}
