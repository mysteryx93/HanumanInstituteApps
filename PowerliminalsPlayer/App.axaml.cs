using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.PowerliminalsPlayer.Views;

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
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainView {DataContext = ViewModelLocator.Main};
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
