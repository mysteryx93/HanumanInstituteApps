using Avalonia;
using Avalonia.ReactiveUI;

namespace HanumanInstitute.Common.Avalonia.App;

public static class AppStarter
{
    public static void Start<TApp>(string[] args) 
        where TApp : Application, new()
        => BuildAvaloniaApp<TApp>()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp<TApp>()
        where TApp : Application, new()
        => AppBuilder.Configure<TApp>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
