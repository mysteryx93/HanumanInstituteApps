namespace HanumanInstitute.Player432Hz.Models;

/// <summary>
/// Represents a file to display in a list.
/// </summary>
public class FileItem
{
    /// <summary>
    /// Initializes a new instance of the FileItem class.
    /// </summary>
    public FileItem() : this(string.Empty, string.Empty)
    { }

    /// <summary>
    /// Initializes a new instance of the FileItem class with specified name and path.
    /// </summary>
    /// <param name="name">The display name of the file.</param>
    /// <param name="path">The full path to the file.</param>
    public FileItem(string name, string path)
    {
        Name = name;
        Path = path;
    }
    
    /// <summary>
    /// The display name of the file.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The full path to the file.
    /// </summary>
    public string Path { get; set; }
}
