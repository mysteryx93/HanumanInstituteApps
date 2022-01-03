using System;
using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.PowerliminalsPlayer.Models;
using MvvmDialogs;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

public static class DialogExtensions
{
    public static async Task<PresetItem?> ShowLoadPresetViewAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel)
    {
        dialog.CheckNotNull(nameof(dialog));

        var viewModel = ViewModelLocator.SelectPreset.Load(false);
        var result = await dialog.ShowDialogAsync(ownerViewModel, viewModel).ConfigureAwait(false);
        return result == true ? viewModel.SelectedItem : null;
    }

    public static async Task<string?> ShowSavePresetViewAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel)
    {
        dialog.CheckNotNull(nameof(dialog));

        var viewModel = ViewModelLocator.SelectPreset.Load(true);
        var result = await dialog.ShowDialogAsync(ownerViewModel, viewModel).ConfigureAwait(false);
        return result == true ? viewModel.PresetName : null;
    }
        
    // public static async Task<bool?> ShowDialogAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel, IModalDialogViewModel viewModel)
    // {
    //     dialog.CheckNotNull(nameof(dialog));
    //
    //     return await Dispatcher.UIThread.InvokeAsync(() => dialog.ShowDialogAsync(ownerViewModel, viewModel));
    // }

    //private static bool IsUiThread() => Thread.CurrentThread == System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread;
}
