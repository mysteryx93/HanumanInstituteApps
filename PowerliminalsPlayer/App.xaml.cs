using System;
using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace PowerliminalsPlayer
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

        public static bool HasExited { get; set; } = false;

        protected override void OnExit(ExitEventArgs e)
        {
            HasExited = true;
            base.OnExit(e);
        }
    }
}
