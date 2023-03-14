using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Converter432hz.Views;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using Splat;

namespace HanumanInstitute.Converter432hz;

public class App : CommonApplication<MainView>
{
    public App()
    {
    }
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override INotifyPropertyChanged InitViewModel() => ViewModelLocator.Main;
    
    protected override void BackgroundInit() => BassDevice.Instance.Init();
}

