using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Player432hz.ViewModels;

namespace HanumanInstitute.Player432hz.Views;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public MainViewModel ViewModel => (MainViewModel)DataContext!;
}
