using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Xaml.Interactions.Core;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using Splat;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Common application class that handles creating the View and ViewModel in parallel.
/// </summary>
/// <typeparam name="TMain">The data type of the main Window.</typeparam>
public abstract class CommonApplication<TMain> : Application
    where TMain : Window
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
        var dialogService = Locator.Current.GetService<IDialogService>()!;
        var themeManager = Locator.Current.GetService<IFluentAvaloniaTheme>()!;

        var desktop = DesktopLifetime;
        if (desktop != null)
        {
            var settings = await AppStarter.AppSettingsLoader!.ConfigureAwait(true);
            InitSettings(settings);
            themeManager.RequestedTheme = settings.Theme.ToString();
            AppStarter.AppSettingsLoader = null;

            var vm = InitViewModel();
            dialogService.Show(null, vm);
            // desktop.MainWindow = desktop.Windows[0];
        }

        GlobalErrorHandler.EndInit(dialogService,
            desktop?.MainWindow?.DataContext as INotifyPropertyChanged);
        RxApp.DefaultExceptionHandler = GlobalErrorHandler.Instance;

        try
        {
            await tBackground.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            GlobalErrorHandler.ShowErrorLog(tBackground.Exception?.InnerException!);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void InitSettings(SettingsDataBase settings)
    {
        if (settings.LicenseKey.HasValue())
        {
            var validator = Locator.Current.GetService<ILicenseValidator>()!;
            settings.IsLicenseValid = validator.Validate(settings.LicenseKey);
        }
        InitLicense(settings);
    }

    protected virtual void InitLicense(SettingsDataBase settings)
    {
        if (!settings.IsLicenseValid)
        {
            settings.ShowInfoOnStartup = true;
            settings.SetFreeLicenseDefaults();
        }
    }

    /// <summary>
    /// Initializes the ViewModel.
    /// </summary>
    /// <returns>The new ViewModel.</returns>
    protected abstract INotifyPropertyChanged InitViewModel();

    /// <summary>
    /// Initializes additional tasks in a background thread while the application loads, if not in design-mode. 
    /// </summary>
    protected virtual void BackgroundInit() { }
}
