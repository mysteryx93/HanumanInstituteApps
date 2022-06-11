using Avalonia;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia.Enums;

namespace HanumanInstitute.Common.Avalonia.App;

public static class AppStarter
{
    public static void Start<TApp>(string[] args)
        where TApp : Application, new()
    {
        try
        {
            BuildAvaloniaApp<TApp>()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Application Error", ex.ToString(), ButtonEnum.Ok,
                Icon.Error);
        }
    } 

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp<TApp>()
        where TApp : Application, new()
        => AppBuilder.Configure<TApp>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
