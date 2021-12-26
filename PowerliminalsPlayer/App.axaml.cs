using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.PowerliminalsPlayer.Views;
using MvvmDialogs.Avalonia;

namespace HanumanInstitute.PowerliminalsPlayer
{
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
                desktop.MainWindow = new MainView {DataContext = ViewModelLocator.Main};
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
