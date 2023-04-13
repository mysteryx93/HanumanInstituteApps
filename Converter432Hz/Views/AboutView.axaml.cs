using Avalonia.Markup.Xaml;
using HanumanInstitute.Avalonia;

namespace HanumanInstitute.Converter432Hz.Views;

public partial class AboutView : CommonWindow<AboutViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
