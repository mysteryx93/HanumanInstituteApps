using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Xaml.Interactions.Core;
using FluentAvalonia.Styling;
using HanumanInstitute.MvvmDialogs;
using Splat;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Common application class that handles creating the View and ViewModel in parallel.
/// </summary>
/// <typeparam name="T">The data type of the main Window.</typeparam>
public abstract class CommonApplication<T> : Application
    where T : Window
{
    /// <summary>
    /// Returns the <see cref="IClassicDesktopStyleApplicationLifetime" />.
    /// </summary>
    public IClassicDesktopStyleApplicationLifetime? DesktopLifetime => ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

    /// <summary>
    /// Once Avalonia framework is initialized, create the View and ViewModel.
    /// </summary>
    public override async void OnFrameworkInitializationCompleted()
    {
#if DEBUG
        // Required by Avalonia XAML editor to recognize custom XAML namespaces. Until they fix the problem.
        GC.KeepAlive(typeof(EventTriggerBehavior));

        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }
#endif

        // We must initialize the ViewModelLocator before setting GlobalErrorHandler.
        // We must set GlobalErrorHandler before View is created.

        // Set DefaultExceptionHelper now but we want to initialize ViewModelLocator later in parallel with View for faster startup.
        GlobalErrorHandler.BeginInit();

        var tBackground = Task.Run(BackgroundInit);

        var desktop = DesktopLifetime;
        if (desktop != null)
        {
            var style = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
            if (style != null)
            {
                style.RequestedTheme = GetTheme().ToString();
            }

            // Initialize View and ViewModel in parallel.
            var t2 = Task.Run(InitViewModel);
            desktop.MainWindow = Activator.CreateInstance<T>();
            await t2.ConfigureAwait(true);
            desktop.MainWindow.DataContext = t2.Result;
        }

        GlobalErrorHandler.EndInit(Locator.Current.GetService<IDialogService>()!,
            desktop?.MainWindow.DataContext as INotifyPropertyChanged);

        await tBackground.ConfigureAwait(true);
        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Initializes the ViewModel.
    /// </summary>
    /// <returns>The new ViewModel.</returns>
    protected abstract INotifyPropertyChanged? InitViewModel();

    /// <summary>
    /// Returns the application theme from the configuration file.
    /// </summary>
    /// <returns>The application theme to initialize.</returns>
    protected abstract AppTheme GetTheme();

    /// <summary>
    /// Initializes additional tasks in a background thread while the application loads, if not in design-mode. 
    /// </summary>
    protected virtual void BackgroundInit() { }
}
