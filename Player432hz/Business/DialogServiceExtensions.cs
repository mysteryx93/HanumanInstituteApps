using System.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Player432hz.Business;

public static class DialogServiceExtensions
{
    public static Task ShowAboutAsync(this IDialogService service, INotifyPropertyChanged owner)
    {
        var vm = service.CreateViewModel<AboutViewModel>();
        return service.ShowDialogAsync(owner, vm);
    }

    public static Task<bool?> ShowSettingsAsync(this IDialogService service, INotifyPropertyChanged owner, AppSettingsData settings)
    {
        var vm = service.CreateViewModel<SettingsViewModel>();
        return service.ShowDialogAsync(owner, vm);
    }
}
