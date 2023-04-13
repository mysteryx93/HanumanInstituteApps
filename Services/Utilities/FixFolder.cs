using System.Collections.Generic;

namespace HanumanInstitute.Services;

public interface FixFolderItem
{
    bool IsFilePath { get; }
    
    int Count { get; }

    string? GetElementAt(int index);

    string? GetElementOf(object item);

    void SetElementAt(int index, string? value);
}

public readonly struct FixFolder<T> : FixFolderItem
    where T : class
{
    public FixFolder(IList<T> folders, bool isFilePath = false, Func<T, string?>? getter = null, Action<T, string?>? setter = null)
    {
        _folders = folders;
        IsFilePath = isFilePath;
        _getter = getter ?? (t => t.ToString());
        _setter = setter;
    }

    private readonly IList<T> _folders;
    private readonly Func<T, string?> _getter;
    private readonly Action<T, string?>? _setter;

    public bool IsFilePath { get; }

    public int Count => _folders.Count;

    public string? GetElementAt(int index) => _getter(_folders[index]);
    
    public string? GetElementOf(object item) => _getter((T)item);

    public void SetElementAt(int index, string? value)
    {
        if (_setter != null)
        {
            _setter(_folders[index], value);
        }
        else
        {
            _folders[index] = (T)(object)value!;
        }  
    } 
}
