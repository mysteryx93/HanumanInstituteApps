using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.PowerliminalsPlayer.Views;

namespace HanumanInstitute.PowerliminalsPlayer;

/// <summary>
/// Maps view models to views.
/// </summary>
public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<AboutViewModel, AboutView>();
        Register<SelectPresetViewModel, SelectPresetView>();
        Register<MainViewModel, MainView>();
        Register<SettingsViewModel, SettingsView>();
    }
}
