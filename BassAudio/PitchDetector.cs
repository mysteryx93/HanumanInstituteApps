using ManagedBass;
// ReSharper disable LocalizableElement

namespace HanumanInstitute.BassAudio;

public class PitchDetector : IPitchDetector
{
    private readonly IFileSystemService _fileSystem;

    public PitchDetector(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task<float> GetPitchAsync(string filePath) =>
        await Task.Run(() => GetPitch(filePath), default).ConfigureAwait(false);
    
    public float GetPitch(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException("Source audio file was not found.", filePath);
        }

        BassDevice.Instance.Init();
        
        var chan = Bass.CreateStream(filePath, Flags: BassFlags.Float | BassFlags.Decode).Valid();
        try
        {
            var chanInfo = Bass.ChannelGetInfo(chan);
            var fft = new float[(int)32768 / 2];
            var fftBuffer = new float[fft.Length];
            var freqStep = (float)chanInfo.Frequency / fft.Length;

            var read = 0;
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

            // var toneFreq = new[]{16.35f,17.32f,18.35f,19.45f,20.6f,21.83f,23.12f,24.5f,25.96f,27.5f,29.14f,30.87f,32.7f,34.65f,36.71f,38.89f,41.2f,43.65f,46.25f,49f,51.91f,55f,58.27f,61.74f,65.41f,69.3f,73.42f,77.78f,82.41f,87.31f,92.5f,98f,103.83f,110f,116.54f,123.47f,130.81f,138.59f,146.83f,155.56f,164.81f,174.61f,185f,196f,207.65f,220f,233.08f,246.94f,261.63f,277.18f,293.66f,311.13f,329.63f,349.23f,369.99f,392f,415.3f,440f,466.16f,493.88f,523.25f,554.37f,587.33f,622.25f,659.25f,698.46f,739.99f,783.99f,830.61f,880f,932.33f,987.77f,1046.5f,1108.73f,1174.66f,1244.51f,1318.51f,1396.91f,1479.98f,1567.98f,1661.22f,1760f,1864.66f,1975.53f,2093f,2217.46f,2349.32f,2489.02f,2637.02f,2793.83f,2959.96f,3135.96f,3322.44f,3520f,3729.31f,3951.07f,4186.01f,4434.92f,4698.63f,4978.03f,5274.04f,5587.65f,5919.91f,6271.93f,6644.88f,7040f,7458.62f,7902.13f};
            var toneFreq = new float[60];
            for (var i = 0; i < 60; i++)
            {
                toneFreq[i] = GetFrequency(i + 20);
            }
            
            // Find the tuning frequency
            var maxSum = 0.0f;
            var maxFreq = 0.0f;
            for (var i = 424.0f; i < 448.1f; i += 0.1f)
            {
                var sum = 0.0f;
                var tones = Array.ConvertAll(toneFreq, x => x * i / 440f);
                var lastFreq = 0f;
                foreach (var freq in tones)
                {
                    if (lastFreq > 0)
                    {
                        var index = (int)(freq / freqStep);
                        var factor = (freq - lastFreq) / freqStep;
                        sum += fft[index] * factor;
                    }
                    lastFreq = freq;
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
