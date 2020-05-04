using System;
using System.Windows;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;

namespace HanumanInstitute.PowerliminalsPlayer.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainView()
        {
            InitializeComponent();
        }
    }
}
