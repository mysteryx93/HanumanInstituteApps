using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace HanumanInstitute.Apps.Tests;

public class FakeDispatcher : IDispatcher
{
    public bool CheckAccess() => true;
    
    public void VerifyAccess() {}
    
    public void Post(Action action, DispatcherPriority priority = default) => action();

    public void Post(SendOrPostCallback action, object? arg, DispatcherPriority priority = default) => action(arg);

    public Task InvokeAsync(Action action, DispatcherPriority priority = default)
    {
        action();
        return Task.CompletedTask;
    }
    
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> function, DispatcherPriority priority = default) =>
        new(function);

    public Task InvokeAsync(Func<Task> function, DispatcherPriority priority = default) =>
        function();

    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function, DispatcherPriority priority = default) =>
        function();
}
