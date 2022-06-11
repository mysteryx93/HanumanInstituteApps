namespace HanumanInstitute.Converter432hz.Models;

/// <summary>
/// Represents a file encoding operation.
/// </summary>
public class ProcessingItem : FileItem
{
    /// <summary>
    /// Initializes a new instance of the ProcessingItem class with specified name and path.
    /// </summary>
    /// <param name="path">The absolute file path.</param>
    /// <param name="relativePath">The display name of the file.</param>
    public ProcessingItem(string path, string relativePath) : base(path, relativePath)
    { }
    
    /// <summary>
    /// Initializes a new instance of the ProcessingItem class from specified FileItem.
    /// </summary>
    /// <param name="file">The FileItem to copy Path and RelativePath from.</param>
    public ProcessingItem(FileItem file) : base(file.Path, file.RelativePath)
    { }

    /// <summary>
    /// Gets or sets the absolute path of the destination file being encoded.
    /// </summary>
    [Reactive]
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the status of the encoding operation.
    /// </summary>
    [Reactive]
    public EncodeStatus Status { get; set; } = EncodeStatus.None;
    
    /// <summary>
    /// Gets or sets the encoding progress in percentage, between 0 and 1.
    /// </summary>
    [Reactive]
    public double ProgressPercent { get; set; }
}
