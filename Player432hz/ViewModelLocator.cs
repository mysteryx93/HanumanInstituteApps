using HanumanInstitute.BassAudio;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Splat;

namespace HanumanInstitute.Player432hz;

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
        container.AddBassAudio();
        container.Register(() => (IDialogService)new DialogService(new DialogManager(
            viewLocator: new ViewLocator(),
            dialogFactory: new DialogFactory().AddMessageBox()),
            viewModelFactory: t => Locator.Current.GetService(t)));
        container.Register(() => (IBassDevice)BassDevice.Instance);
            
        // ViewModels
        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<IPlaylistViewModelFactory, PlaylistViewModelFactory>();
        SplatRegistrations.RegisterLazySingleton<IFilesListViewModel, FilesListViewModel>();
        SplatRegistrations.RegisterLazySingleton<IPlayerViewModel, PlayerViewModel>();
        SplatRegistrations.Register<AboutViewModel>();

        // Business
        SplatRegistrations.RegisterLazySingleton<ISettingsProvider<AppSettingsData>, AppSettingsProvider>();
        SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
        SplatRegistrations.RegisterLazySingleton<IPlaylistPlayer, PlaylistPlayer>();
        SplatRegistrations.Register<IFileLocator, FileLocator>();
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static IFilesListViewModel FilesList => Locator.Current.GetService<IFilesListViewModel>()!;
    public static IPlayerViewModel Player => Locator.Current.GetService<IPlayerViewModel>()!;
    public static AboutViewModel About => Locator.Current.GetService<AboutViewModel>()!;

    public static void Cleanup()
    {
    }
}
