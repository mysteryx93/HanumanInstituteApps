using Avalonia.Markup.Xaml;

namespace HanumanInstitute.YangDownloader.Views;

public partial class SettingsView : CommonWindow<SettingsViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
