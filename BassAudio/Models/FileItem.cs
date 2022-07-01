using ReactiveUI;

namespace HanumanInstitute.BassAudio;

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
    [Reactive]
    public string Path { get; set; }

    /// <summary>
    /// The display name of the file.
    /// </summary>
    public string RelativePath
    {
        get => _relativePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _relativePath, value);
            this.RaisePropertyChanged(nameof(Text));
        }
    }
    private string _relativePath;

    /// <summary>
    /// Gets or sets the file audio pitch.
    /// </summary>
    public float? Pitch
    {
        get => _pitch;
        set
        {
            this.RaiseAndSetIfChanged(ref _pitch, value);
            this.RaisePropertyChanged(nameof(Text));
        }
    }
    private float? _pitch;

    /// <summary>
    /// Gets the display text.
    /// </summary>
    public string Text => Pitch.HasValue ? $"[{Pitch:F1}] {RelativePath}" : RelativePath;

    /// <summary>
    /// Gets the tooltip to display for the item.
    /// </summary>
    public virtual string ToolTip => Path;
}
