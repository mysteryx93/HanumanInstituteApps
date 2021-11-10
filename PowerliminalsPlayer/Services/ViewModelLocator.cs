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
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonServiceLocator;
using CommonServiceLocator.WindsorAdapter;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.PowerliminalsPlayer.Models;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using MvvmDialogs;

namespace HanumanInstitute.PowerliminalsPlayer.Services
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Required for designer integration")]
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            using var container = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            // Services
            container.AddCommonServices();
            container.Register(Component.For<IDialogService>().ImplementedBy<DialogService>()
                .DependsOn(Dependency.OnValue("dialogTypeLocator", new AppDialogTypeLocator())).LifeStyle.Transient);

            // ViewModels
            container.Register(Component.For<MainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Transient);
            container.Register(Component.For<SelectPresetViewModel>().ImplementedBy<SelectPresetViewModel>()
                .DependsOn().LifeStyle.Transient);

            // Business
            container.Register(Component.For<ISettingsProvider<AppSettingsData>>().ImplementedBy<AppSettingsProvider>().LifeStyle.Singleton);
            container.Register(Component.For<IAppPathService>().ImplementedBy<AppPathService>().LifeStyle.Singleton);
        }

        public static MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public static SelectPresetViewModel SelectPreset => ServiceLocator.Current.GetInstance<SelectPresetViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
