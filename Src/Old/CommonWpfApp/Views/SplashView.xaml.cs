using System;
using System.Windows;
using HanumanInstitute.CommonWpfApp.ViewModels;

namespace HanumanInstitute.CommonWpfApp.Views
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashView : Window
    {
        public SplashViewModel ViewModel => (SplashViewModel)DataContext;

        public SplashView()
        {
            InitializeComponent();
        }

        bool _canClose = false;

        private void CloseAnimation_Completed(object sender, EventArgs e)
        {
            _canClose = true;
            // Close();
            Dispatcher.InvokeShutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
            }
        }
    }
}
