using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using GalaSoft.MvvmLight.Threading;

namespace PowerliminalsPlayer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        public static bool HasExited = false;

        protected override void OnExit(ExitEventArgs e) {
            HasExited = true;
            base.OnExit(e);
        }
    }
}
