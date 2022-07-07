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

        if (!Avalonia.Controls.Design.IsDesignMode)
        {
            // For now, load in AppSettingsProvider constructor otherwise it's not displaying the data. Need to investigate.
            //Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!.Load();
            BassDevice.Instance.Init();
        }
    }
    protected override INotifyPropertyChanged? InitViewModel() => ViewModelLocator.Main;
}
