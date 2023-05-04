using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using HanumanInstitute.Avalonia;

namespace HanumanInstitute.Apps;

/// <inheritdoc />
public class ClipboardService : IClipboardService
{
    protected virtual IClipboard? Clipboard => _clipboard ??= Application.Current.GetTopLevel()?.Clipboard;
    private IClipboard? _clipboard;

    /// <inheritdoc />
    public Task<string?> GetTextAsync() => Clipboard?.GetTextAsync() ?? Task.FromResult<string?>(null);

    /// <inheritdoc />
    public Task SetTextAsync(string? text) => Clipboard?.SetTextAsync(text) ?? Task.CompletedTask;

    /// <inheritdoc />
    public Task ClearAsync() => Clipboard?.ClearAsync() ?? Task.CompletedTask;

    /// <inheritdoc />
    public Task SetDataObjectAsync(IDataObject data) => Clipboard?.SetDataObjectAsync(data) ?? Task.CompletedTask;

    /// <inheritdoc />
    public Task<string[]> GetFormatsAsync() => Clipboard?.GetFormatsAsync() ?? Task.FromResult(Array.Empty<string>());

    /// <inheritdoc />
    public Task<object?> GetDataAsync(string format) => Clipboard?.GetDataAsync(format) ?? Task.FromResult<object?>(null);
}
