using Avalonia.Markup.Xaml;
using HanumanInstitute.Avalonia;

namespace HanumanInstitute.PowerliminalsPlayer.Views;

/// <summary>
/// Interaction logic for SelectPresetView.xaml
/// </summary>
public partial class SelectPresetView : CommonWindow<SelectPresetViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}