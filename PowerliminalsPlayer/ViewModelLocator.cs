using Avalonia.Controls;
using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
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
        container.Register(() => (IDialogService)new DialogService(new DialogManager(
                viewLocator: new ViewLocator(),
                dialogFactory: new DialogFactory().AddFluent()),
            viewModelFactory: t => Locator.Current.GetService(t)));
        container.RegisterLazySingleton<IBassDevice>(() => BassDevice.Instance);
        container.RegisterLazySingleton<IFluentAvaloniaTheme>(() => 
            new FluentAvaloniaThemeWrapper(AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()!));

        // ViewModels
        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<AboutViewModel>();
        SplatRegistrations.Register<SelectPresetViewModel>();
        SplatRegistrations.Register<SettingsViewModel>();

        // Business
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>("Init");
        container.Register(() => 
            Design.IsDesignMode ? new AppSettingsProviderDesign() : Locator.Current.GetService<ISettingsProvider<AppSettingsData>>("Init"));
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
        SplatRegistrations.RegisterLazySingleton<IPathFixer, AppPathFixer>();
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static AboutViewModel About => Locator.Current.GetService<AboutViewModel>()!;
    public static SelectPresetViewModel SelectPreset => Locator.Current.GetService<SelectPresetViewModel>()!;
    public static SettingsViewModel Settings => Locator.Current.GetService<SettingsViewModel>()!;
    public static ISettingsProvider<AppSettingsData> SettingsProvider => Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!;

    public static void Cleanup()
    {
    }
}
