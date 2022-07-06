using Avalonia;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia.Enums;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Handles application startup in a generic way. Static methods must still exist in Program.cs, but they can call these implementations. 
/// </summary>
public static class AppStarter
{
    /// <summary>
    /// Call this from Program.Main.
    /// </summary>
    public static async void Start<TApp>(string[] args)
        where TApp : Application, new()
    {
        try
        {
            BuildAvaloniaApp<TApp>()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Application Error", ex.ToString(), 
                ButtonEnum.Ok, Icon.Error);
        }
    }

    /// <summary>
    /// Call this from Program.BuildAvaloniaApp.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp<TApp>()
        where TApp : Application, new()
        => AppBuilder.Configure<TApp>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
