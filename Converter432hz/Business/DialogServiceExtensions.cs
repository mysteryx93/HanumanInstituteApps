using System.ComponentModel;
using System.Threading;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Converter432hz.Business;

/// <summary>
/// Exposes application dialogs in a strongly-typed way.
/// </summary>
public static class DialogServiceExtensions
{
    private static SemaphoreSlim _semaphoreAskFileAction = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Shows the AskFileAction dialog.
    /// </summary>
    /// <param name="service">The IDialogService on which to attach the extension method.</param>
    /// <param name="ownerViewModel">A view model that represents the owner window of the dialog.</param>
    /// <param name="filePath">The path to the existing file.</param>
    /// <returns>The selected action, or Skip if the dialog was closed.</returns>
    public static async Task<AskFileActionViewModel> ShowAskFileActionAsync(this IDialogService service,
        INotifyPropertyChanged ownerViewModel, string filePath, CancellationToken cancellationToken = default)
    {
        // Allow a single instance at the same time.
        await _semaphoreAskFileAction.WaitAsync(cancellationToken);

        try
        {
            var vm = service.CreateViewModel<AskFileActionViewModel>();
            vm.FilePath = filePath;
            if (cancellationToken.IsCancellationRequested)
            {
                vm.Items.SelectedValue = FileExistsAction.Cancel;
            }
            else
            {
                await service.ShowDialogAsync(ownerViewModel, vm).ConfigureAwait(false);
            }
            return vm;
        }
        finally
        {
            _semaphoreAskFileAction.Release();
        }
    }

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
