// /*
//   In App.xaml:
//   <Application.Resources>
//       <vm:ViewModelLocator xmlns:vm="clr-namespace:HanumanInstitute.Player432hz"
//                            x:Key="Locator" />
//   </Application.Resources>
//   
//   In the View:
//   DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
//
//   You can also use Blend to do all this with the tool's support.
//   See http://www.galasoft.ch/mvvm
// */
//
// using System;
// using Castle.MicroKernel.Registration;
// using Castle.Windsor;
// using CommonServiceLocator;
// using CommonServiceLocator.WindsorAdapter;
// using HanumanInstitute.Common.Services;
// using HanumanInstitute.Common.Avalonia.App;
// using HanumanInstitute.Player432hz.Business;
// using MvvmDialogs;
// using Component = Castle.MicroKernel.Registration.Component;
//
// namespace HanumanInstitute.Player432hz.ViewModels
// {
//     /// <summary>
//     /// This class contains static references to all the view models in the
//     /// application and provides an entry point for the bindings.
//     /// </summary>
//     [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Required for designer integration")]
//     public class ViewModelLocator
//     {
//         /// <summary>
//         /// Initializes a new instance of the ViewModelLocator class.
//         /// </summary>
//         public ViewModelLocator()
//         {
//             using var container = new WindsorContainer();
//             ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
//
//             // Services
//             container.AddCommonServices();
//             container.Register(Component.For<IDialogService>().ImplementedBy<DialogService>()
//                 .DependsOn(Dependency.OnValue("dialogTypeLocator", new AppDialogTypeLocator())).LifeStyle.Transient);
//
//             // ViewModels
//             container.Register(Component.For<MainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Transient);
//             container.Register(Component.For<IPlaylistViewModelFactory>().ImplementedBy<PlaylistViewModelFactory>().LifeStyle.Transient);
//             container.Register(Component.For<IFilesListViewModel>().ImplementedBy<FilesListViewModel>().LifeStyle.Singleton);
//             container.Register(Component.For<IPlayerViewModel>().ImplementedBy<PlayerViewModel>().LifeStyle.Singleton);
//
//             // Business
//             container.Register(Component.For<ISettingsProvider<SettingsData>>().ImplementedBy<AppSettingsProvider>().LifeStyle.Singleton);
//             container.Register(Component.For<IAppPathService>().ImplementedBy<AppPathService>().LifeStyle.Singleton);
//             container.Register(Component.For<IFileLocator>().ImplementedBy<FileLocator>().LifeStyle.Transient);
//             container.Register(Component.For<IPlaylistPlayer>().ImplementedBy<PlaylistPlayer>().LifeStyle.Singleton);
//         }
//
//         public static MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
//         public static IFilesListViewModel FilesList => ServiceLocator.Current.GetInstance<IFilesListViewModel>();
//         public static IPlayerViewModel Player => ServiceLocator.Current.GetInstance<IPlayerViewModel>();
//
//         public static void Cleanup()
//         {
//         }
//     }
// }
