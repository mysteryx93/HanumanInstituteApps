using Avalonia.Controls;
using FluentAvalonia.Styling;
using HanumanInstitute.BassAudio;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Downloads;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.YangDownloader.Business;
using HanumanInstitute.YangDownloader.Views;
using LazyCache.Splat;
using Splat;

namespace HanumanInstitute.YangDownloader;

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
        container.AddDownloads();
        container.AddLazyCache();
        
        var viewLocator = new StrongViewLocator()
            .Register<AboutViewModel, AboutView>()
            .Register<EncodeSettingsViewModel, EncodeSettingsView>()
            .Register<MainViewModel, MainView>()
            .Register<SettingsViewModel, SettingsView>();
        
        container.Register(() => (IDialogService)new DialogService(new DialogManager(
            viewLocator: new ViewLocator(),
            dialogFactory: new DialogFactory().AddFluent()),
            viewModelFactory: t => Locator.Current.GetService(t)));
        container.Register(() => (IBassDevice)BassDevice.Instance);
        container.Register<IEncoderService>(() => new EncoderService());
        SplatRegistrations.Register<IMediaMuxer, MediaMuxer>();
        SplatRegistrations.Register<IMediaEncoder, MediaEncoder>();
        SplatRegistrations.Register<IMediaInfoReader, MediaInfoReader>();
        SplatRegistrations.Register<IMediaScript, MediaScript>();
        container.RegisterLazySingleton<IFluentAvaloniaTheme>(() => 
            new FluentAvaloniaThemeWrapper(AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()!));

        // ViewModels
        SplatRegistrations.Register<MainViewModel>("Init");
        container.Register(() => 
            Design.IsDesignMode ? new MainViewModelDesign() : Locator.Current.GetService<MainViewModel>("Init"));
        SplatRegistrations.Register<AboutViewModel>();
        SplatRegistrations.Register<SettingsViewModel>();
        SplatRegistrations.Register<EncodeSettingsViewModel>();

        // Business
        SplatRegistrations.RegisterLazySingleton<IAppInfo, AppInfo>();
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>("Init");
        container.Register(() => 
            Design.IsDesignMode ? new AppSettingsProviderDesign() : Locator.Current.GetService<ISettingsProvider<AppSettingsData>>("Init"));
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static AboutViewModel About => Locator.Current.GetService<AboutViewModel>()!;
    public static EncodeSettingsViewModel EncodeSettings => Locator.Current.GetService<EncodeSettingsViewModel>()!;
    public static SettingsViewModel Settings => Locator.Current.GetService<SettingsViewModel>()!;
    public static ISettingsProvider<AppSettingsData> SettingsProvider => Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!;

    public static void Cleanup()
    {
    }
}
