using Avalonia.Markup.Xaml;

namespace HanumanInstitute.PowerliminalsPlayer.Views;

public partial class AboutView : CommonWindow<AboutViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}

