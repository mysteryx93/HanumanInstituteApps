using Xunit.Abstractions;

namespace HanumanInstitute.Common.Avalonia.App.Tests;

/// <summary>
/// Base class for test classes.
/// </summary>
public class TestsBase
{
    protected readonly ITestOutputHelper _output;

    public TestsBase(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Allows using a lambda expression after ??= operator.
    /// </summary>
    protected T Init<T>(Func<T> func) => func();
}
