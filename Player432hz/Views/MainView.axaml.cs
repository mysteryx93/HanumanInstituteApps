using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Player432hz.Views;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : CommonWindow<MainViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
