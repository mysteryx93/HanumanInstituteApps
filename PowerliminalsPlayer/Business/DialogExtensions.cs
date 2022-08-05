using System.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

public static class DialogExtensions
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
    
    public static async Task<PresetItem?> ShowLoadPresetViewAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel)
    {
        dialog.CheckNotNull(nameof(dialog));

        var viewModel = ViewModelLocator.SelectPreset.Load(false);
        var result = await dialog.ShowDialogAsync(ownerViewModel, viewModel).ConfigureAwait(true);
        return result == true ? viewModel.SelectedItem : null;
    }

    public static async Task<string?> ShowSavePresetViewAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel)
    {
        dialog.CheckNotNull(nameof(dialog));

        var viewModel = ViewModelLocator.SelectPreset.Load(true);
        var result = await dialog.ShowDialogAsync(ownerViewModel, viewModel).ConfigureAwait(true);
        return result == true ? viewModel.PresetName : null;
    }
}
