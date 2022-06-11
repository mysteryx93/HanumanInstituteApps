using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Converter432hz.Views;
using HanumanInstitute.MvvmDialogs.Avalonia;
using ReactiveUI;
using Splat;

namespace HanumanInstitute.Converter432hz;

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

            GlobalErrorHandler.Set(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
