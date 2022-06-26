namespace HanumanInstitute.BassAudio;

/// <summary>
/// Represents a supported file extension.
/// </summary>
public class FileExtension
{
    /// <summary>
    /// Initializes a new instance of the FileExtension class.
    /// </summary>
    /// <param name="name">The name of the file format.</param>
    /// <param name="extensions"> A list of file extensions.</param>
    public FileExtension(string name, IEnumerable<string> extensions)
    {
        Name = name;
        Extensions = extensions.ToList();
    }

    /// <summary>
    /// The name of the file format.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// A list of file extensions.
    /// </summary>
    public IList<string> Extensions { get; }

    /// <summary>
    /// Returns a string representation of the file formats and extensions.
    /// </summary>
    public override string ToString() => Name + " | " + string.Join(';', Extensions);
}
