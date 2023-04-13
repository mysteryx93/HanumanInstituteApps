using System.ComponentModel;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.Player432Hz.Views;

namespace HanumanInstitute.Player432Hz;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override INotifyPropertyChanged InitViewModel() => ViewModelLocator.Main;

    protected override void BackgroundInit()
    {
        BassDevice.Instance.InitPlugins();
        BassDevice.Instance.VerifyPlugins();
    }
}
