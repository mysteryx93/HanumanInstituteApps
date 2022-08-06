using System.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Converter432hz.Business;

/// <summary>
/// Exposes application dialogs in a strongly-typed way.
/// </summary>
public static class DialogServiceExtensions
{
    /// <summary>
    /// Shows the AdvancedSettings dialog.
    /// </summary>
    /// <param name="service">The IDialogService on which to attach the extension method.</param>
    /// <param name="ownerViewModel">A view model that represents the owner window of the dialog.</param>
    /// <param name="settings">The settings object. Edited settings will be applied directly to this object.</param>
    public static async Task ShowAdvancedSettingsAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel,
        EncodeSettings settings)
    {
        var vm = service.CreateViewModel<AdvancedSettingsViewModel>();
        Cloning.CopyAllFields(settings, vm.Settings);

        if (await service.ShowDialogAsync(ownerViewModel, vm).ConfigureAwait(false) == true)
        {
            Cloning.CopyAllFields(vm.Settings, settings);
        }
    }
    
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
