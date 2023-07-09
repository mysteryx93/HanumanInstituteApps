using HanumanInstitute.Apps.AdRotator;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Player432Hz.ViewModels;

public sealed class AboutViewModel : AboutViewModelBase<AppSettingsData>
{
    public AboutViewModel(IAppInfo appInfo, IEnvironmentService environment, ISettingsProvider<AppSettingsData> settings, 
        IHanumanInstituteHttpClient httpClient, ILicenseValidator licenseValidator, IDialogService dialogService, IAdRotatorViewModel adRotator) :
        base(appInfo, environment, settings, httpClient, licenseValidator, dialogService, adRotator)
    {
    }
}
