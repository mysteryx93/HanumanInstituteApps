using Avalonia.Markup.Xaml;
using HanumanInstitute.Avalonia;

namespace HanumanInstitute.Converter432Hz.Views;

public partial class SettingsView : CommonWindow<SettingsViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
