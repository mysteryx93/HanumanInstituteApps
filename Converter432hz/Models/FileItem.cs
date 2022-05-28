using ReactiveUI;

namespace HanumanInstitute.Converter432hz.Models;

/// <summary>
/// Represents a file to display in a list.
/// </summary>
public class FileItem : ReactiveObject
{
    /// <summary>
    /// Initializes a new instance of the FileItem class with specified name and path.
    /// </summary>
    /// <param name="path">The absolute file path.</param>
    /// <param name="relativePath">The file path to encode to, relative to the destination. Can be the file name, or include a subfolder.</param>
    public FileItem(string path, string relativePath)
    {
        Path = path;
        RelativePath = relativePath;
    }
    
    /// <summary>
    /// The full path to the file.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// The display name of the file.
    /// </summary>
    public string RelativePath { get; private set; }
}
