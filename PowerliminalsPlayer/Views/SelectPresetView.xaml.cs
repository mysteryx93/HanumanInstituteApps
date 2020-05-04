using System;
using System.Windows;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;

namespace HanumanInstitute.PowerliminalsPlayer.Views
{
    /// <summary>
    /// Interaction logic for SelectPresetView.xaml
    /// </summary>
    public partial class SelectPresetView : Window
    {
        public SelectPresetViewModel ViewModel => (SelectPresetViewModel)DataContext;

        public SelectPresetView()
        {
            InitializeComponent();
            ViewModel.RequestClose += delegate { this.Close(); };
        }
    }
}
