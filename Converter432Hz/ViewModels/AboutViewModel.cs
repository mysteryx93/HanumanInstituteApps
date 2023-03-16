using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Converter432Hz.ViewModels;

public class AboutViewModel : AboutViewModelBase<AppSettingsData>
{
    public AboutViewModel(IAppInfo appInfo, IEnvironmentService environment,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService, ILicenseValidator licenseValidator, IDialogService dialogService) :
        base(appInfo, environment, settings, updateService, licenseValidator, dialogService)
    {
    }
}
