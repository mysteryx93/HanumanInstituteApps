using Avalonia.Threading;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Splat;

namespace HanumanInstitute.Converter432hz;

/// <summary>
/// This class contains static references to all the view models in the
/// application and provides an entry point for the bindings.
/// </summary>
public static class ViewModelLocator
{
    /// <summary>
    /// Initializes a new instance of the ViewModelLocator class.
    /// </summary>
    static ViewModelLocator()
    {
        var container = Locator.CurrentMutable;
            
        // Services
        container.AddCommonAvaloniaApp();
        container.AddCommonServices();
        container.AddBassAudio();
        
        container.Register<IDialogService>(() => new DialogService(
            viewModelFactory: x => Locator.Current.GetService(x)));
        container.Register<IBassDevice>(() => BassDevice.Instance);
        container.Register<IDispatcher>(() => Dispatcher.UIThread);
        SplatRegistrations.Register<GlobalErrorHandler>();
            
        // ViewModels
        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<AskFileActionViewModel>();
        SplatRegistrations.Register<AdvancedSettingsViewModel>();

        // Business
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>();
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
        SplatRegistrations.Register<IFileLocator, FileLocator>();
        SplatRegistrations.Register<IEncoderService, EncoderService>();
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static AskFileActionViewModel AskFileAction => Locator.Current.GetService<AskFileActionViewModel>()!;
    public static AdvancedSettingsViewModel AdvancedSettings => Locator.Current.GetService<AdvancedSettingsViewModel>()!;
    
    public static void Cleanup()
    {
    }
}
