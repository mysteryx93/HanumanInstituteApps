using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace HanumanInstitute.Common.Avalonia.App.Tests;

public class FakeDispatcher : IDispatcher
{
    public bool CheckAccess() => true;
    
    public void VerifyAccess() {}
    
    public void Post(Action action, DispatcherPriority priority = DispatcherPriority.Normal) => action();

    public Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        action();
        return Task.CompletedTask;
    }
    
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> function, DispatcherPriority priority = DispatcherPriority.Normal) =>
        new(function);

    public Task InvokeAsync(Func<Task> function, DispatcherPriority priority = DispatcherPriority.Normal) =>
        function();

    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function, DispatcherPriority priority = DispatcherPriority.Normal) =>
        function();
}
