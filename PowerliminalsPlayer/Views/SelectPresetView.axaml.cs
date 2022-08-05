using Avalonia.Markup.Xaml;

namespace HanumanInstitute.PowerliminalsPlayer.Views;

/// <summary>
/// Interaction logic for SelectPresetView.xaml
/// </summary>
public partial class SelectPresetView : CommonWindow<SelectPresetViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}