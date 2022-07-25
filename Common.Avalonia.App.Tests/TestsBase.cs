using Xunit.Abstractions;
using Moq;

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

    /// <summary>
    /// Initializes a mock of specified type.
    /// </summary>
    /// <param name="action">A lambda expression to configure the mock.</param>
    /// <typeparam name="T">The type of mock to create.</typeparam>
    /// <returns>The newly-created mock.</returns>
    protected Mock<T> InitMock<T>(Action<Mock<T>> action)
        where T : class
    {
        var mock = new Mock<T>();
        action(mock);
        return mock;
    } 
}
