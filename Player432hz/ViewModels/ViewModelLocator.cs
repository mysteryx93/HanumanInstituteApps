/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:HanumanInstitute.Player432hz"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonServiceLocator;
using CommonServiceLocator.WindsorAdapter;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Player432hz.Business;
using GalaSoft.MvvmLight;
using MvvmDialogs;
using Component = Castle.MicroKernel.Registration.Component;

namespace HanumanInstitute.Player432hz.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var container = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            if (ViewModelBase.IsInDesignModeStatic)
            {
                container.Register(Component.For<ISettingsProvider>().ImplementedBy<SettingsProviderDesign>().LifeStyle.Singleton);
            }
            else
            {
                container.Register(Component.For<ISettingsProvider>().ImplementedBy<SettingsProvider>().LifeStyle.Singleton);
            }

            // Services
            container.AddCommonServices();
            container.Register(Component.For<IDialogService>().ImplementedBy<DialogService>()
                .DependsOn(Dependency.OnValue("dialogTypeLocator", new AppDialogTypeLocator())).LifeStyle.Transient);

            // ViewModels
            container.Register(Component.For<MainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Transient);
            container.Register(Component.For<IPlaylistViewModelFactory>().ImplementedBy<PlaylistViewModelFactory>().LifeStyle.Transient);
            container.Register(Component.For<IFilesListViewModel>().ImplementedBy<FilesListViewModel>().LifeStyle.Singleton);
            container.Register(Component.For<IPlayerViewModel>().ImplementedBy<PlayerViewModel>().LifeStyle.Singleton);

            // Business
            container.Register(Component.For<IAppPathService>().ImplementedBy<AppPathService>().LifeStyle.Singleton);
            container.Register(Component.For<IFileLocator>().ImplementedBy<FileLocator>().LifeStyle.Transient);
            container.Register(Component.For<IPlaylistPlayer>().ImplementedBy<PlaylistPlayer>().LifeStyle.Singleton);

            container.Dispose();
        }

#pragma warning disable CA1822
        public SettingsProvider Settings => ServiceLocator.Current.GetInstance<ISettingsProvider>() as SettingsProvider;
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public IFilesListViewModel FilesList => ServiceLocator.Current.GetInstance<IFilesListViewModel>();
        public IPlayerViewModel Player => ServiceLocator.Current.GetInstance<IPlayerViewModel>();
#pragma warning restore CA1822

        public static void Cleanup()
        {
        }
    }
}
