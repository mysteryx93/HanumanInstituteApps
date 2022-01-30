using System;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

public class FileItem : ReactiveObject, IComparable<FileItem>
{
    public FileItem(string fullPath, string display)
    {
        _fullPath = fullPath;
        _display = display;
    }

    public string FullPath
    {
        get => _fullPath;
        set => this.RaiseAndSetIfChanged(ref _fullPath, value);
    }
    private string _fullPath;

    public string Display
    {
        get => _display;
        set => this.RaiseAndSetIfChanged(ref _display, value);
    }
    private string _display;

    public int CompareTo(FileItem? other) =>
        string.Compare(Display, other?.Display ?? string.Empty, StringComparison.CurrentCulture);
}
