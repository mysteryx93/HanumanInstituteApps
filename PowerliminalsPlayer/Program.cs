using HanumanInstitute.Common.Avalonia.App;
using Splat;

// ReSharper disable ClassNeverInstantiated.Global
namespace HanumanInstitute.PowerliminalsPlayer;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => AppStarter.Start<App>(args,
        () => ViewModelLocator.SettingsProvider.Value,
        () => ViewModelLocator.AppPathService.UnhandledExceptionLogPath);

    // Avalonia configuration, don't remove; also used by visual designer.
    // ReSharper disable once MemberCanBePrivate.Global
    public static AppBuilder BuildAvaloniaApp() => AppStarter.BuildAvaloniaApp<App>();
}
