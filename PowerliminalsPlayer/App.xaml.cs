using System;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.PowerliminalsPlayer.Views;

namespace HanumanInstitute.PowerliminalsPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppStarter.RunWithSplash(
                HanumanInstitute.PowerliminalsPlayer.Properties.Resources.AppIcon,
                () => new MainView());
            base.OnStartup(e);
        }

        public static bool HasExited { get; set; } = false;

        protected override void OnExit(ExitEventArgs e)
        {
            HasExited = true;
            base.OnExit(e);
        }
    }
}
