using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HanumanInstitute.Converter432Hz.Views;

public partial class AskFileActionView : CommonWindow<AskFileActionViewModel>
{
    protected override void Initialize() => AvaloniaXamlLoader.Load(this);
}
