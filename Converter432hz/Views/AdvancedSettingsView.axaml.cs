using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Converter432hz.Views;

public partial class AdvancedSettingsView : Window
{
    public AdvancedSettingsView()
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

