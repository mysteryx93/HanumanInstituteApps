using Avalonia.Markup.Xaml;

namespace HanumanInstitute.PowerliminalsPlayer.Views;

public partial class SettingsView : CommonWindow<SettingsViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
