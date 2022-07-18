using System.ComponentModel;
using HanumanInstitute.BassAudio;
using ExtensionMethods = HanumanInstitute.Common.Services.ExtensionMethods;

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
    public static async Task ShowEncodeSettingsAsync(this IDialogService service, INotifyPropertyChanged ownerViewModel,
        EncodeSettings settings)
    {
        var vm = service.CreateViewModel<EncodeSettingsViewModel>();
        // vm.Settings = settings.Clone();
        vm.Settings.Rate = settings.Rate;
        vm.Settings.Speed = settings.Speed;
        vm.Settings.PitchFrom = settings.PitchFrom;
        vm.Settings.PitchTo = settings.PitchTo;
        vm.Settings.AntiAlias = settings.AntiAlias;
        vm.Settings.AntiAliasLength = settings.AntiAliasLength;
        vm.Settings.MaxThreads = settings.MaxThreads;

        if (await service.ShowDialogAsync(ownerViewModel, vm).ConfigureAwait(false) == true)
        {
            settings.Rate = vm.Settings.Rate;
            settings.Speed = vm.Settings.Speed;
            settings.PitchFrom = vm.Settings.PitchFrom;
            settings.PitchTo = vm.Settings.PitchTo;
            settings.AntiAlias = vm.Settings.AntiAlias;
            settings.AntiAliasLength = vm.Settings.AntiAliasLength;
            settings.MaxThreads = vm.Settings.MaxThreads;
        }
    }
}
