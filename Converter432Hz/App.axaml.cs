using System.ComponentModel;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Converter432Hz.Views;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;

namespace HanumanInstitute.Converter432Hz;

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

