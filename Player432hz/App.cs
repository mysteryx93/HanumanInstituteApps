using System;
using System.Windows;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.Player432hz.Views;
using Res = HanumanInstitute.Player432hz.Properties.Resources;

namespace HanumanInstitute.Player432hz
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
