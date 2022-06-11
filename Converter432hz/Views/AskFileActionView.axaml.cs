using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Converter432hz.Views;

public partial class AskFileActionView : Window
{
    public AskFileActionView()
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

