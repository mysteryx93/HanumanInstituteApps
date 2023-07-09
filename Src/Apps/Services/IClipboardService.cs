using Avalonia.Input;

namespace HanumanInstitute.Apps;

/// <summary>
/// Provides access to the clipboard.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Gets text from the clipboard.
    /// </summary>
    Task<string?> GetTextAsync();

    /// <summary>
    /// Sets text to the clipboard.
    /// </summary>
    /// <param name="text">The text to set.</param>
    Task SetTextAsync(string? text);

    /// <summary>
    /// Clears the clipboard.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Sets data into the clipboard.
    /// </summary>
    /// <param name="data">The data to set.</param>
    Task SetDataObjectAsync(IDataObject data);

    /// <summary>
    /// Gets data formats stored into the clipboard.
    /// </summary>
    Task<string[]> GetFormatsAsync();

    /// <summary>
    /// Gets data from the clipboard of specified format.
    /// </summary>
    /// <param name="format">The format of data to get.</param>
    Task<object?> GetDataAsync(string format);
}
