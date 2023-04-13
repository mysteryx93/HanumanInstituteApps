using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Services;

/// <summary>
/// Application settings for design view.
/// </summary>
public abstract class SettingsProviderDesign<T> : ISettingsProvider<T>
    where T : class, new()
{
    protected SettingsProviderDesign(T value)
    {
        Value = value;
    }

    public T Value { get; set; }

#pragma warning disable 67
    public event EventHandler? Changed;
    public event EventHandler? Saving;
#pragma warning restore

    /// <inheritdoc />
    public T Load() => Value;

    /// <inheritdoc />
    public T Load(string path) => Value;

    /// <inheritdoc />
    public void Save()
    {
    }

    /// <inheritdoc />
    public void Save(string path)
    {
    }
}
