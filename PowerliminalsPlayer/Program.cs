using System;
using HanumanInstitute.Common.Avalonia.App;

// ReSharper disable ClassNeverInstantiated.Global

namespace HanumanInstitute.PowerliminalsPlayer;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => AppStarter.Start<App>(args);
    
    // , "/Assets/App.png", () => new MainView() { DataContext = ViewModelLocator.Main});

    /*[STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();*/
}
