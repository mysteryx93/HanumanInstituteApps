using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HanumanInstitute.YangDownloader.Views;

public partial class EncodeSettingsView : Window
{
    public EncodeSettingsView()
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

