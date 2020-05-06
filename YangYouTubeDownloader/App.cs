using System;
using System.Windows;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.YangYouTubeDownloader.Views;
using Res = HanumanInstitute.YangYouTubeDownloader.Properties.Resources;

namespace HanumanInstitute.YangYouTubeDownloader
{
    public class App : Application
    {
        /// <summary>
        /// Application startup point.
        /// </summary>
        [STAThread]
        public static void Main(string[] _)
        {
            AppStarter.RunWithSplash(Res.AppIcon, typeof(MainView));
        }
    }
}
