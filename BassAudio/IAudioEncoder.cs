namespace HanumanInstitute.BassAudio;

/// <summary>
/// Encodes an audio using BASS.
/// </summary>
public interface IAudioEncoder
{
    /// <summary>
    /// Starts an encoding process asynchronously.
    /// </summary>
    /// <param name="file">Information about the file to encode.</param>
    /// <param name="settings">The encoding settings.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the task.</param>
    /// <exception cref="System.IO.FileNotFoundException">File was not found.</exception>
    /// <exception cref="ManagedBass.BassException">File is invalid or there was an error while processing the file.</exception>
    Task StartAsync(ProcessingItem file, EncodeSettings settings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts an encoding process.
    /// </summary>
    /// <param name="file">Information about the file to encode.</param>
    /// <param name="settings">The encoding settings.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the task.</param>
    /// <exception cref="System.IO.FileNotFoundException">File was not found.</exception>
    /// <exception cref="ManagedBass.BassException">File is invalid or there was an error while processing the file.</exception>
    void Start(ProcessingItem file, EncodeSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the list of sample rates supported for specified audio format.
    /// </summary>
    /// <param name="format">The audio format to get supported sample rates for.</param>
    /// <returns>A list of supported sample rates.</returns>
    public int[] GetSupportedSampleRates(EncodeFormat format);
}
