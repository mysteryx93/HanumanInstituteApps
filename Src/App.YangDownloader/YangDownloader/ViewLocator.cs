using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.YangDownloader.Views;

namespace HanumanInstitute.YangDownloader;

/// <summary>
/// Maps view models to views.
/// </summary>
public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<AboutViewModel, AboutView>();
        Register<EncodeSettingsViewModel, EncodeSettingsView>();
        Register<MainViewModel, MainView>();
        Register<SettingsViewModel, SettingsView>();
    }
}
