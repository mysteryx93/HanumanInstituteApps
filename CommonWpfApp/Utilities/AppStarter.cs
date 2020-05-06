using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Castle.Core.Internal;
using GalaSoft.MvvmLight.Threading;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp.ViewModels;
using HanumanInstitute.CommonWpfApp.Views;

namespace HanumanInstitute.CommonWpfApp
{
    public class AppStarter
    {
        public static void RunWithSplash(Bitmap splashImage, Type mainWindow, string resourceFile = "App.xaml")
        {
            splashImage.CheckNotNull(nameof(splashImage));
            mainWindow.CheckDerivesFrom(typeof(Window), nameof(mainWindow));

            // Show splash screen.
            var splashVM = CommonWpfApp.ViewModels.ViewModelLocator.Splash;
            var SplashThread = new Thread(() => ShowSplashThread(splashVM, splashImage));
            SplashThread.SetApartmentState(ApartmentState.STA);
            //SplashThread.IsBackground = true;
            SplashThread.Priority = ThreadPriority.AboveNormal;
            SplashThread.Name = "Splash Screen";
            SplashThread.Start();
            Thread.Yield();

            // Initialize app.
            DispatcherHelper.Initialize();
            Application.LoadComponent(new System.Uri("/App.xaml", System.UriKind.Relative));
            var app = Application.Current;

            // Initialize main window.
            // Note: using App.StartupUri would not allow us to call LoadCompleted when the main window is ready.
            var view = mainWindow.CreateInstance<Window>();
            app.MainWindow = view;
            view.Show();
            view.Activate();
            splashVM.LoadCompleted();

            app.Run();
        }

        private static void ShowSplashThread(SplashViewModel splashVM, Bitmap splashImage)
        {
            splashVM.Image = splashImage;
            var splashView = new SplashView()
            {
                DataContext = splashVM
            };
            splashView.Show();
            Dispatcher.Run();
        }
    }
}
