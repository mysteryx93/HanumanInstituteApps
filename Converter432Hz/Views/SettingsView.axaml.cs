using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Converter432Hz.Views;

public partial class SettingsView : CommonWindow<SettingsViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
