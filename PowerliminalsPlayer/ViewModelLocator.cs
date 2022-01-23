using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Business;
using HanumanInstitute.PowerliminalsPlayer.Models;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using MvvmDialogs;
using MvvmDialogs.Avalonia;
using MvvmDialogs.DialogTypeLocators;
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
        container.Register(() => (IDialogService)new DialogService());
            
        // ViewModels
        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<SelectPresetViewModel>();

        // Business
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>();
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static SelectPresetViewModel SelectPreset => Locator.Current.GetService<SelectPresetViewModel>()!;

    public static void Cleanup()
    {
    }
}
