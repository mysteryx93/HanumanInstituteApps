using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using HanumanInstitute.PowerliminalsPlayer.Views;

namespace HanumanInstitute.PowerliminalsPlayer;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.SvgImage));
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MediaPlayer.Avalonia.Bass.BassDevice.Instance.Init();
            desktop.MainWindow = new MainView
            {
                DataContext = ViewModelLocator.Main
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
