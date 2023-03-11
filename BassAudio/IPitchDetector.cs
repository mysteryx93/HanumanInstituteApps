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
    /// <param name="useCache">True to cache the data, false to calculate without cache.</param>
    /// <returns>The audio pitch between 424 and 448.</returns>
    Task<float> GetPitchAsync(string filePath, bool useCache = true);
    /// <summary>
    /// Measures the pitch of an audio file, between 424hz and 448hz. Most music is between 440-442hz.
    /// </summary>
    /// <param name="filePath">The path of the file to measure.</param>
    /// <param name="useCache">True to cache the data, false to calculate without cache.</param>
    /// <returns>The audio pitch between 424 and 448.</returns>
    float GetPitch(string filePath, bool useCache = true);
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
