using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Handles application startup in a generic way. Static methods must still exist in Program.cs, but they can call these implementations. 
/// </summary>
public static class AppStarter
{
    /// <summary>
    /// Gets or sets the task to initialize ViewModelLocator, settings, and return the configured theme.
    /// </summary>
    public static Task<SettingsDataBase>? AppSettingsLoader { get; set; }
    
    /// <summary>
    /// Call this from Program.Main.
    /// </summary>
    public static void Start<TApp>(string[] args, Func<SettingsDataBase> getSettings, Func<string?>? logPath)
        where TApp : Application, new()
    {
        PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Avalonia);
        try
        {
            GlobalErrorHandler.LogPath = logPath?.Invoke();
            // Initialize ViewModelLocator and load settings in parallel.
            AppSettingsLoader = Task.Run(getSettings);
            
            BuildAvaloniaApp<TApp>()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            GlobalErrorHandler.ShowErrorLog(ex);
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
