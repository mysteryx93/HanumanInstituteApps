using HanumanInstitute.Common.Services;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.PowerliminalsPlayer.Business;
using HanumanInstitute.PowerliminalsPlayer.Models;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using MvvmDialogs;
using MvvmDialogs.Avalonia;
using Splat;

namespace HanumanInstitute.PowerliminalsPlayer;

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
        container.AddCommonServices();
        container.RegisterLazySingleton<IDialogService>(() => new DialogService());
        container.RegisterLazySingleton<IBassDevice>(() => BassDevice.Instance);
            
        // ViewModels
        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<SelectPresetViewModel>();

        // Business
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>();
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
        SplatRegistrations.RegisterLazySingleton<IFolderPathFixer, FolderPathFixer>();
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static SelectPresetViewModel SelectPreset => Locator.Current.GetService<SelectPresetViewModel>()!;

    public static void Cleanup()
    {
    }
}
