using System;
using HanumanInstitute.Common.Services;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace HanumanInstitute.Common.Avalonia.App.Tests;

public class FakeSettingsProvider<T> : ISettingsProvider<T>
    where T : class, new()
{
    public virtual T Value { get; set; } = new T();

    public event EventHandler? Loaded;
    public event EventHandler? Saved;

    public virtual T Load()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
        return Value;
    }

    public virtual T Load(string path)
    {
        Loaded?.Invoke(this, EventArgs.Empty);
        return Value;
    }

    public virtual void Save()
    {
        Saved?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Save(string path)
    {
        Saved?.Invoke(this, EventArgs.Empty);
    }
}
