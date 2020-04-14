/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:PowerliminalsPlayer"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using System.Windows;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonServiceLocator;
using CommonServiceLocator.WindsorAdapter;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp;
//using HanumanInstitute.YangYouTubeDownloader.Business;
//using HanumanInstitute.YangYouTubeDownloader.Views;
using MvvmDialogs;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static ViewModelLocator s_instance;
        public static ViewModelLocator Instance => s_instance ?? (s_instance = (ViewModelLocator)Application.Current.FindResource("Locator"));

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var container = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            container.AddCommonServices();

            container.Register(Component.For<MainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Transient);
            //container.Register(Component.For<SelectPresetViewModel>().ImplementedBy<SelectPresetViewModel>()
            //    .DependsOn().LifeStyle.Transient);

            container.Register(Component.For<IDialogService>().ImplementedBy<DialogService>()
                .DependsOn(Dependency.OnValue("dialogTypeLocator", new AppDialogTypeLocator())).LifeStyle.Transient);
            //container.Register(Component.For<AppSettingsProvider>().ImplementedBy<AppSettingsProvider>().LifeStyle.Singleton);
            //container.Register(Component.For<IAppPathService>().ImplementedBy<AppPathService>().LifeStyle.Singleton);
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        //public SelectPresetViewModel SelectPreset => ServiceLocator.Current.GetInstance<SelectPresetViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
