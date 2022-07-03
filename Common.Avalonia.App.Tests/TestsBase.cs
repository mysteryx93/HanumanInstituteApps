using Xunit.Abstractions;

namespace HanumanInstitute.Common.Avalonia.App.Tests;

/// <summary>
/// Base class for test classes.
/// </summary>
public class TestsBase
{
    protected ITestOutputHelper Output { get; }

    public TestsBase(ITestOutputHelper output) => Output = output;

    /// <summary>
    /// Allows using a lambda expression after ??= operator.
    /// </summary>
    protected T Init<T>(Func<T> func) => func();
}
