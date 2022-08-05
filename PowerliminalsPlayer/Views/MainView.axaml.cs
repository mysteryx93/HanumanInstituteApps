using Avalonia.Markup.Xaml;

namespace HanumanInstitute.PowerliminalsPlayer.Views;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : CommonWindow<MainViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);  
}
