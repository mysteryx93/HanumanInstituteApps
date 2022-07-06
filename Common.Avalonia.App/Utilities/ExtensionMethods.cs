using ReactiveUI;
using Splat;

namespace HanumanInstitute.Common.Avalonia.App;

public static class ExtensionMethods
{
    public static ReactiveCommand<TParam, TResult> HandleExceptions<TParam, TResult>(this ReactiveCommand<TParam, TResult> command)
    {
        command.ThrownExceptions.Subscribe(Exception_Raised);
        return command;
    }

    private static void Exception_Raised(Exception e)
    {
        var handler = Locator.Current.GetService<GlobalErrorHandler>()!;
        handler.ShowError(e);
    }
}
