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
        public static void RunWithSplash(Bitmap splashImage, Func<Window> createWindow)
        {
            splashImage.CheckNotNull(nameof(splashImage));
            createWindow.CheckNotNull(nameof(createWindow));

            // Show splash screen.
            var splashVM = CommonWpfApp.ViewModels.ViewModelLocator.Splash;
            var splashThread = new Thread(() => ShowSplashThread(splashVM, splashImage));
            splashThread.SetApartmentState(ApartmentState.STA);
            //SplashThread.IsBackground = true;
            splashThread.Priority = ThreadPriority.AboveNormal;
            splashThread.Name = "Splash Screen";
            splashThread.Start();
            Thread.Yield();

            // Initialize main window.
            // Note: using App.StartupUri would not allow us to call LoadCompleted when the main window is ready.
            var view = createWindow();
            view.Show();
            view.Activate();
            Application.Current.MainWindow = view;
            splashVM.LoadCompleted();
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
