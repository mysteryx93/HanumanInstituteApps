using System.ComponentModel;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Apps;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.YangDownloader.Views;

namespace HanumanInstitute.YangDownloader;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override INotifyPropertyChanged InitViewModel() => ViewModelLocator.Main;
    
    protected override void BackgroundInit()
    {
        BassDevice.Instance.InitNoSound();
        BassDevice.Instance.VerifyPlugins();
    }
}
