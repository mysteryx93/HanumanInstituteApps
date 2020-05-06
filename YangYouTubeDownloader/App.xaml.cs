using System;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.YangYouTubeDownloader.Views;

namespace HanumanInstitute.YangYouTubeDownloader
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
                YangYouTubeDownloader.Properties.Resources.AppIcon,
                () => new MainView());
            base.OnStartup(e);
        }
    }
}
