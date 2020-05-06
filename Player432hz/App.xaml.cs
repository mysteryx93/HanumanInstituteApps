using System;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Player432hz.Views;

namespace HanumanInstitute.Player432hz
{
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppStarter.RunWithSplash(
                Player432hz.Properties.Resources.AppIcon,
                () => new MainView());
            base.OnStartup(e);
        }
    }
}
