using System.ComponentModel;
using HanumanInstitute.BassAudio;

namespace HanumanInstitute.YangDownloader.Business;

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
    public static async Task ShowEncodeSettingsAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel,
        EncodeSettings settings)
    {
        var vm = service.CreateViewModel<EncodeSettingsViewModel>();
        vm.Settings = settings.Clone();
        //ExtensionMethods.CopyAll(settings, vm.Settings);

        if (await service.ShowDialogAsync(ownerViewModel, vm).ConfigureAwait(false) == true)
        {
            ExtensionMethods.CopyAllFields(vm.Settings, settings);
        }
    }
}
