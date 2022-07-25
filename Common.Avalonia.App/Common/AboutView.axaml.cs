using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Common.Avalonia.App;

public partial class AboutView : Window
{
    public AboutView()
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
}

