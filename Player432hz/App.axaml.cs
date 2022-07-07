using System.ComponentModel;
using Splat;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.Player432hz.Views;

namespace HanumanInstitute.Player432hz;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        if (!Avalonia.Controls.Design.IsDesignMode)
        {
            Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!.Load();
            BassDevice.Instance.Init();
        }
    }

    protected override INotifyPropertyChanged? InitViewModel() => ViewModelLocator.Main;
}
