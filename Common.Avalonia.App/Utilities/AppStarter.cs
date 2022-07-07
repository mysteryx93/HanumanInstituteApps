using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Handles application startup in a generic way. Static methods must still exist in Program.cs, but they can call these implementations. 
/// </summary>
public static class AppStarter
{
    /// <summary>
    /// Call this from Program.Main.
    /// </summary>
    public static void Start<TApp>(string[] args, Func<string>? logPath)
        where TApp : Application, new()
    {
        try
        {
            BuildAvaloniaApp<TApp>()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if (logPath != null)
            {
                // Dump error to log file and open default text editor.
                var log = logPath();
                System.IO.File.WriteAllText(log, ex.ToString());
                var notepad = new Process
                {
                    StartInfo = new ProcessStartInfo(log)
                    {
                        UseShellExecute = true
                    }
                };
                notepad.Start();
                Thread.Sleep(TimeSpan.FromSeconds(10));
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
            .UseReactiveUI();
}
