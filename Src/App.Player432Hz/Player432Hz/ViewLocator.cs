using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.Player432Hz.Views;

namespace HanumanInstitute.Player432Hz;

/// <summary>
/// Maps view models to views.
/// </summary>
public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<AboutViewModel, AboutView>();
        Register<MainViewModel, MainView>();
        Register<SettingsViewModel, SettingsView>();
    }
}
