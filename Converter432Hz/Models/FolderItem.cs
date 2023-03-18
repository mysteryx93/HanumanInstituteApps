namespace HanumanInstitute.Converter432Hz.Models;

/// <summary>
/// Represents a folder to display in a list.
/// </summary>
public class FolderItem : FileItem
{
    /// <summary>
    /// Initializes a new instance of the FolderItem class with specified name and path.
    /// </summary>
    /// <param name="path">The absolute folder path.</param>
    /// <param name="relativePath">The folder to create in the destination.</param>
    public FolderItem(string path, string relativePath) : base(path, relativePath)
    {
    }

    /// <summary>
    /// Gets or sets the list of files contained in the folder.
    /// </summary>
    public ObservableCollection<FileItem> Files { get; } = new();

    /// <inheritdoc />
    public override string ToolTip =>
        Path + Environment.NewLine +
        Files.Count switch
        {
            0 => "Empty folder",
            1 => "1 file",
            _ => Files.Count + " files"
        };
}
