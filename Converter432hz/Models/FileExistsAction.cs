namespace HanumanInstitute.Converter432hz.Models;

/// <summary>
/// Represents the action to take when the destination file already exists. 
/// </summary>
public enum FileExistsAction
{
    /// <summary>
    /// Ask what to do.
    /// </summary>
    Ask,
    /// <summary>
    /// Skip the file.
    /// </summary>
    Skip,
    /// <summary>
    /// Overwrite the destination file.
    /// </summary>
    Overwrite,
    /// <summary>
    /// Rename the new file.
    /// </summary>
    Rename,
    /// <summary>
    /// Cancel the job.
    /// </summary>
    Cancel
}
