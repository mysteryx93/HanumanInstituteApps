using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using HanumanInstitute.CommonServices;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using MvvmDialogs;

namespace HanumanInstitute.PowerliminalsPlayer.Services
{
    public static class DialogExtensions
    {
        public static async Task<SelectPresetViewModel?> ShowSelectPresetViewAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel, bool modeSave)
        {
            dialog.CheckNotNull(nameof(dialog));

            var viewModel = ViewModelLocator.SelectPreset.Load(modeSave);
            var result = await ShowDialogAsync(dialog, ownerViewModel, viewModel).ConfigureAwait(false);
            return result == true ? viewModel : null;
        }

        public static async Task<bool?> ShowDialogAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel, IModalDialogViewModel viewModel)
        {
            dialog.CheckNotNull(nameof(dialog));

            return await Dispatcher.CurrentDispatcher.InvokeAsync(() => dialog.ShowDialog(ownerViewModel, viewModel));
        }

        //private static bool IsUiThread() => Thread.CurrentThread == System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread;
    }
}
