using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Svg.Skia;
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
        GC.KeepAlive(typeof(SvgImage));
#endif
        // We must initialize the ViewModelLocator before setting GlobalErrorHandler.
        // We must set GlobalErrorHandler before View is created.

        // Set DefaultExceptionHelper now but we want to initialize ViewModelLocator later in parallel with View for faster startup.
        GlobalErrorHandler.BeginInit();

        var desktop = DesktopLifetime;
        if (desktop != null)
        {
            // Initialize View and ViewModel/ ViewModelLocator in parallel.
            var t1 = Task.Run(InitViewModel);
            desktop.MainWindow = Activator.CreateInstance<T>();
            await t1.ConfigureAwait(true);
            desktop.MainWindow.DataContext = t1.Result;
        }

        GlobalErrorHandler.EndInit(Locator.Current.GetService<IDialogService>()!, desktop?.MainWindow.DataContext as INotifyPropertyChanged);

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Initializes the ViewModel.
    /// </summary>
    /// <returns>The new ViewModel.</returns>
    protected virtual INotifyPropertyChanged? InitViewModel() => null;
}
