using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

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
