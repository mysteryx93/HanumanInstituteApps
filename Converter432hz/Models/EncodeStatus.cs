namespace HanumanInstitute.Converter432hz.Models;

/// <summary>
/// Represents the status of the encoding operation.
/// </summary>
public enum EncodeStatus
{
    None,
    Processing,
    Completed,
    Error,
    Cancelled,
    Skip
}
