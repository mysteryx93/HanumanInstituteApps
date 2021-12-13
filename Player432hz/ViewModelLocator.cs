using HanumanInstitute.Common.Services;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using MvvmDialogs;
using MvvmDialogs.Avalonia;
using MvvmDialogs.DialogTypeLocators;
using Splat;

namespace HanumanInstitute.Player432hz
{
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
            container.Register(() => (IDialogService)new DialogService(dialogTypeLocator: new NamingConventionDialogTypeLocator()
            {
                ViewSuffix = "View"
            }));
            
            // ViewModels
            SplatRegistrations.Register<MainViewModel>();
            SplatRegistrations.Register<IPlaylistViewModelFactory, PlaylistViewModelFactory>();
            SplatRegistrations.RegisterLazySingleton<IFilesListViewModel, FilesListViewModel>();
            SplatRegistrations.RegisterLazySingleton<IPlayerViewModel, PlayerViewModel>();

            // Business
            SplatRegistrations.RegisterLazySingleton<ISettingsProvider<SettingsData>, AppSettingsProvider>();
            SplatRegistrations.RegisterLazySingleton<IAppPathService, AppPathService>();
            SplatRegistrations.RegisterLazySingleton<IPlaylistPlayer, PlaylistPlayer>();
            SplatRegistrations.Register<IFileLocator, FileLocator>();
            
            SplatRegistrations.SetupIOC();
        }

        public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
        public static IFilesListViewModel FilesList => Locator.Current.GetService<IFilesListViewModel>()!;
        public static IPlayerViewModel Player => Locator.Current.GetService<IPlayerViewModel>()!;

        public static void Cleanup()
        {
        }
    }
}
