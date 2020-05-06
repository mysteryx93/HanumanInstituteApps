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
using System.Diagnostics.CodeAnalysis;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonServiceLocator;
using CommonServiceLocator.WindsorAdapter;
using GalaSoft.MvvmLight;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp;
using MvvmDialogs;

namespace HanumanInstitute.CommonWpfApp.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    [SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Required for designer integration")]
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
        }

        public static SplashViewModel Splash => new SplashViewModel();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
