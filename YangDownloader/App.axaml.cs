using System.ComponentModel;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.YangDownloader.Views;

namespace HanumanInstitute.YangDownloader;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override INotifyPropertyChanged InitViewModel() => ViewModelLocator.Main;
    
    protected override void BackgroundInit()
    {
        BassDevice.Instance.Init(0);
        BassDevice.Instance.VerifyPlugins();
    }
}
