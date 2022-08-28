namespace HanumanInstitute.BassAudio;

/// <summary>
/// Provides audio pitch-detection. The data is cached for 1 day.
/// </summary>
public interface IPitchDetector
{
    /// <summary>
    /// Measures the pitch of an audio file, between 424hz and 448hz. Most music is between 440-442hz.
    /// </summary>
    /// <param name="filePath">The path of the file to measure.</param>
    /// <returns>The audio pitch between 424 and 448.</returns>
    Task<float> GetPitchAsync(string filePath);
    /// <summary>
    /// Measures the pitch of an audio file, between 424hz and 448hz. Most music is between 440-442hz.
    /// </summary>
    /// <param name="filePath">The path of the file to measure.</param>
    /// <returns>The audio pitch between 424 and 448.</returns>
    float GetPitch(string filePath);
}
