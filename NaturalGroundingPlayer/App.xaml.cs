using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using NaturalGroundingPlayer;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static bool HasExited = false;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            var container = new WindsorContainer();

            // Register servivces.
            container.Register(Component.For<IDialogService>().ImplementedBy<DialogService>());

            // Register ViewModels.
            container.Register(Component.For<MainWindow>().ImplementedBy<MainWindow>());


            var root = container.Resolve<MainWindow>();
            root.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            HasExited = true;
            base.OnExit(e);
        }
    }
}
