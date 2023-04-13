using System.Text.Json.Serialization.Metadata;
using Avalonia.Controls;
using FluentAvalonia.Styling;
using HanumanInstitute.BassAudio;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.Player432Hz.Views;
using LazyCache.Splat;
using Splat;

namespace HanumanInstitute.Player432Hz;

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
        container.AddCommonAvaloniaApp<AppSettingsData>();
        container.AddCommonServices();
        container.AddBassAudio();
        container.AddLazyCache();
        
        var viewLocator = new StrongViewLocator()
            .Register<AboutViewModel, AboutView>()
            .Register<MainViewModel, MainView>()
            .Register<SettingsViewModel, SettingsView>();
        
        container.Register(() => (IDialogService)new DialogService(new DialogManager(
            viewLocator: viewLocator,
            dialogFactory: new DialogFactory().AddFluent()),
            viewModelFactory: t => Locator.Current.GetService(t)));
        container.Register(() => (IBassDevice)BassDevice.Instance);
        container.RegisterLazySingleton<IFluentAvaloniaTheme>(() => 
            new FluentAvaloniaThemeWrapper(AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()!));
            
        // ViewModels
        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<IPlaylistViewModelFactory, PlaylistViewModelFactory>();
        SplatRegistrations.RegisterLazySingleton<IFilesListViewModel, FilesListViewModel>();
        SplatRegistrations.RegisterLazySingleton<IPlayerViewModel, PlayerViewModel>();
        SplatRegistrations.Register<AboutViewModel>();
        SplatRegistrations.Register<SettingsViewModel>();

        // Business
        // container.RegisterWithDesign<ISettingsProvider<AppSettingsData>, AppSettingsProvider, AppSettingsProviderDesign>();
        SplatRegistrations.RegisterLazySingleton<IAppInfo, AppInfo>();
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>("Init");
        container.Register(() => 
            Design.IsDesignMode ? new AppSettingsProviderDesign() : Locator.Current.GetService<ISettingsProvider<AppSettingsData>>("Init"));
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
        SplatRegistrations.RegisterLazySingleton<IPlaylistPlayer, PlaylistPlayer>();
        SplatRegistrations.Register<IFileLocator, FileLocator>();
        container.RegisterLazySingleton<IJsonTypeInfoResolver>(() => SourceGenerationContext.Default);

        SplatRegistrations.SetupIOC();
    }

    // public static void RegisterWithDesign<TInterface, TDesign>(this IMutableDependencyResolver resolver)
    //     where TDesign : TInterface, new() => RegisterWithDesign<TInterface, TInterface, TDesign>(resolver);
    //
    // public static void RegisterWithDesign<TInterface, TConcrete, TDesign>(this IMutableDependencyResolver resolver)
    //     where TDesign : TInterface, new()
    // {
    //     SplatRegistrations.Register<TInterface, TConcrete>("Init");
    //     resolver.Register<TInterface>(() =>
    //         Design.IsDesignMode ? new TDesign() : Locator.Current.GetService<TInterface>("Init"));
    // }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static IFilesListViewModel FilesList => Locator.Current.GetService<IFilesListViewModel>()!;
    public static IPlayerViewModel Player => Locator.Current.GetService<IPlayerViewModel>()!;
    public static AboutViewModel About => Locator.Current.GetService<AboutViewModel>()!;
    public static SettingsViewModel Settings => Locator.Current.GetService<SettingsViewModel>()!;
    public static ISettingsProvider<AppSettingsData> SettingsProvider => Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!;
    public static IAppPathService AppPathService => Locator.Current.GetService<IAppPathService>()!;

    public static void Cleanup()
    {
    }
}
