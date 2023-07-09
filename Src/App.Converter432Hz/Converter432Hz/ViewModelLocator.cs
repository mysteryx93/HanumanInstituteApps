using System.Text.Json.Serialization.Metadata;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using HanumanInstitute.Converter432Hz.Views;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using LazyCache.Splat;
using Splat;

namespace HanumanInstitute.Converter432Hz;

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
            .Register<AskFileActionViewModel, AskFileActionView>()
            .Register<MainViewModel, MainView>()
            .Register<SettingsViewModel, SettingsView>();

        container.Register<IDialogService>(() => new DialogService(
            viewModelFactory: x => Locator.Current.GetService(x), 
            dialogManager: new DialogManager(viewLocator: viewLocator,
                dialogFactory: new DialogFactory().AddFluent())));
        container.Register<IBassDevice>(() => BassDevice.Instance);
        container.Register<IDispatcher>(() => Dispatcher.UIThread);
        SplatRegistrations.Register<IFluentAvaloniaTheme, FluentAvaloniaThemeWrapper>();

        // ViewModels
        SplatRegistrations.Register<MainViewModel>("Init");
        container.Register(() => 
            Design.IsDesignMode ? new MainViewModelDesign() : Locator.Current.GetService<MainViewModel>("Init"));
        SplatRegistrations.Register<AboutViewModel>();
        SplatRegistrations.Register<SettingsViewModel>();
        SplatRegistrations.Register<AskFileActionViewModel>();
        
        // Business
        SplatRegistrations.RegisterLazySingleton<IAppInfo, AppInfo>();
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>("Init");
        container.Register(() => 
            Design.IsDesignMode ? new AppSettingsProviderDesign() : Locator.Current.GetService<ISettingsProvider<AppSettingsData>>("Init"));
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
        SplatRegistrations.Register<IFileLocator, FileLocator>();
        SplatRegistrations.Register<IEncoderService, EncoderService>();
        container.RegisterLazySingleton<IJsonTypeInfoResolver>(() => SourceGenerationContext.Default);
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static AboutViewModel About => Locator.Current.GetService<AboutViewModel>()!;
    public static AskFileActionViewModel AskFileAction => Locator.Current.GetService<AskFileActionViewModel>()!;
    public static SettingsViewModel Settings => Locator.Current.GetService<SettingsViewModel>()!;
    public static ISettingsProvider<AppSettingsData> SettingsProvider => Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!;
    public static IAppPathService AppPathService => Locator.Current.GetService<IAppPathService>()!;

    public static void Cleanup()
    {
    }
}
