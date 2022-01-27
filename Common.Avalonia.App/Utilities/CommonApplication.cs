using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MvvmDialogs;
using MvvmDialogs.Avalonia;

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
        GC.KeepAlive(typeof(DialogService));
#endif

        var desktop = DesktopLifetime;
        if (desktop != null)
        {
            // Initialize View and ViewModel in parallel.
            var t1 = InitViewModelAsync();
            desktop.MainWindow = Activator.CreateInstance<T>();
            await t1.ConfigureAwait(true);
            desktop.MainWindow.DataContext = t1.Result;
            await InitCompleted();
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Initializes the ViewModel.
    /// </summary>
    /// <returns>The new ViewModel.</returns>
    protected virtual Task<INotifyPropertyChanged?> InitViewModelAsync() => Task.FromResult<INotifyPropertyChanged?>(null);

    /// <summary>
    /// Perform post-initialization steps after the main window is shown and bound to the ViewModel.
    /// </summary>
    protected virtual Task InitCompleted() => Task.CompletedTask;
}
