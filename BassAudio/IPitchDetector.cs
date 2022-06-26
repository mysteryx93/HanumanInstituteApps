namespace HanumanInstitute.BassAudio;

public interface IPitchDetector
{
    Task<float> GetPitchAsync(string filePath);
    float GetPitch(string filePath);
}
