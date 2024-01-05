using HanumanInstitute.Converter432Hz.Views;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace HanumanInstitute.Converter432Hz;

/// <summary>
/// Maps view models to views.
/// </summary>
public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<AboutViewModel, AboutView>();
        Register<AskFileActionViewModel, AskFileActionView>();
        Register<MainViewModel, MainView>();
        Register<SettingsViewModel, SettingsView>();
    }
}
