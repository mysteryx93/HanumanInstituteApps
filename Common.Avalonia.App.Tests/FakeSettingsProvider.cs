using HanumanInstitute.Common.Services;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace HanumanInstitute.Common.Avalonia.App.Tests;

public class FakeSettingsProvider<T> : ISettingsProvider<T>
    where T : class, new()
{
    public virtual T Value { get; set; } = new();

    public event EventHandler? Changed;
    public event EventHandler? Saving;

    public virtual T Load()
    {
        Changed?.Invoke(this, EventArgs.Empty);
        return Value;
    }

    public virtual T Load(string path)
    {
        Changed?.Invoke(this, EventArgs.Empty);
        return Value;
    }

    public virtual void Save()
    {
        Saving?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Save(string path)
    {
        Saving?.Invoke(this, EventArgs.Empty);
    }
}
