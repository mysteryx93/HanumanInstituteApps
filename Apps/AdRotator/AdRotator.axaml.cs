using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Apps.AdRotator;

public partial class AdRotator : UserControl
{
    public AdRotator()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

