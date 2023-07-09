using Avalonia.Markup.Xaml;
using HanumanInstitute.Avalonia;

namespace HanumanInstitute.Converter432Hz.Views;

public partial class AskFileActionView : CommonWindow<AskFileActionViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
