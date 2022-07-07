using System.ComponentModel;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.PowerliminalsPlayer.Views;
using Splat;

namespace HanumanInstitute.PowerliminalsPlayer;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
        
        Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!.Load();
        BassDevice.Instance.Init();
    }
    protected override INotifyPropertyChanged? InitViewModel() => ViewModelLocator.Main;
}
