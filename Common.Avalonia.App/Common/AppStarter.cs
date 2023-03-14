using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Windowing;
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
            // Initialize ViewModelLocator and load settings in parallel.
            AppSettingsLoader = Task.Run(getSettings);
            
            BuildAvaloniaApp<TApp>()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if (logPath != null)
            {
                // Dump error to log file and open default text editor.
                var log = logPath();
                if (log.HasValue())
                {
                    System.IO.File.WriteAllText(log, ex.ToString());
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(log)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                    // Text editor gets killed after 1 second in the IDE, but stays open if app is run directly. 
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
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
            .UseReactiveUI()
            .UseFAWindowing();

}
