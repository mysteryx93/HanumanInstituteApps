using System.Diagnostics;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using Splat;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// A global error handler that displays a message box when unhandled errors occur.
/// </summary>
public class GlobalErrorHandler : IObserver<Exception>
{
    private readonly IDialogService _dialogService;

    /// <summary>
    /// Sets the global error handler to display all errors with specified owner Window.
    /// </summary>
    /// <param name="owner">The Window owner in which to display the errors.</param>
    public static void Set(Window owner)
    {
        var errHandler = Locator.Current.GetService<GlobalErrorHandler>()!;
        errHandler.Owner = owner;
        RxApp.DefaultExceptionHandler = errHandler;
    }
    
    /// <summary>
    /// Initializes a new instance of the GlobalErrorHandler class.
    /// </summary>
    public GlobalErrorHandler(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    /// <summary>
    /// Gets or sets the Window owner in which to display error messages.
    /// </summary>
    public Window? Owner { get; set; }
    
    /// <inheritdoc />
    public async void OnNext(Exception error)
    {
        if (Debugger.IsAttached) Debugger.Break();

        var _ = await _dialogService.ShowMessageBoxAsync(Owner, error.ToString(), "Application Error", MessageBoxButton.Ok, MessageBoxImage.Error);
    }

    /// <inheritdoc />
    public void OnError(Exception error)
    {
        if (Debugger.IsAttached) Debugger.Break();

    }

    /// <inheritdoc />
    public void OnCompleted()
    {
        if (Debugger.IsAttached) Debugger.Break();

    }
}
