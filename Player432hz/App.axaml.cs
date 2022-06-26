using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.Player432hz.Views;

namespace HanumanInstitute.Player432hz;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        GC.KeepAlive(typeof(DialogService));
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainView
            {
                DataContext = ViewModelLocator.Main
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
