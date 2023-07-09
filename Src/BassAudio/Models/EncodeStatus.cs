namespace HanumanInstitute.BassAudio;

/// <summary>
/// Represents the status of the encoding operation.
/// </summary>
public enum EncodeStatus
{
    /// <summary>
    /// No status.
    /// </summary>
    None,
    /// <summary>
    /// Encoding is processing.
    /// </summary>
    Processing,
    /// <summary>
    /// Encoding is completed.
    /// </summary>
    Completed,
    /// <summary>
    /// Encoding failed.
    /// </summary>
    Error,
    /// <summary>
    /// Encoding was cancelled.
    /// </summary>
    Cancelled,
    /// <summary>
    /// Encoding was skipped by the user.
    /// </summary>
    Skip
}
