using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace PowerliminalsPlayer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static bool HasExited = false;

        protected override void OnExit(ExitEventArgs e) {
            HasExited = true;
            base.OnExit(e);
        }
    }
}
