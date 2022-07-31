using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Player432hz.Views;

public partial class SettingsView : CommonWindow<SettingsViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
