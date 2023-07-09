namespace HanumanInstitute.BassAudio;

/// <summary>
/// Provides audio pitch-detection.
/// </summary>
public interface IPitchDetector
{
    /// <summary>
    /// Measures the pitch of an audio file, between 424Hz and 448Hz. Most music is between 440-442Hz.
    /// </summary>
    /// <param name="filePath">The path of the file to measure.</param>
    /// <returns>The audio pitch between 424 and 448.</returns>
    Task<float> GetPitchAsync(string filePath);
    /// <summary>
    /// Measures the pitch of an audio file, between 424Hz and 448Hz. Most music is between 440-442Hz.
    /// It is recommended to use the Async method when using multiple passes.
    /// </summary>
    /// <param name="filePath">The path of the file to measure.</param>
    /// <returns>The audio pitch between 424 and 448.</returns>
    float GetPitch(string filePath);
    /// <summary>
    /// Gets or sets the list of sample rates at which analysis will be done. This overrides <see cref="AnalyzePasses"/>. 
    /// </summary>
    IEnumerable<int>? AnalyzeSampleRates { get; set; }
    /// <summary>
    /// Gets or sets how many times the operation will be performed at various sample rates. A higher value will
    /// give better results but multiply the calculation time.
    /// This can make a significant performance difference with GetPitch().
    /// With GetPitchAsync, the passes are done in separate tasks in parallel.
    /// </summary>
    /// <returns></returns>
    public int AnalyzePasses { get; set; }
}
