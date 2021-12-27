using System;
using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Common.Avalonia.App.Tests;

public class FakeSettingsProvider<T> : ISettingsProvider<T>
    where T : class, new()
{
    public T Value { get; set; } = new T();

    public event EventHandler? Loaded;
    public event EventHandler? Saved;

    public T Load()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
        return Value;
    }

    public T Load(string path)
    {
        Loaded?.Invoke(this, EventArgs.Empty);
        return Value;
    }

    public void Save()
    {
        Saved?.Invoke(this, EventArgs.Empty);
    }

    public void Save(string path)
    {
        Saved?.Invoke(this, EventArgs.Empty);
    }
}
