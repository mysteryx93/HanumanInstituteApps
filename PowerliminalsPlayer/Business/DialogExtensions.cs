using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using MvvmDialogs;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    public static class DialogExtensions
    {
        public static async Task<SelectPresetViewModel?> ShowSelectPresetViewAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel, bool modeSave)
        {
            dialog.CheckNotNull(nameof(dialog));

            var viewModel = ViewModelLocator.SelectPreset.Load(modeSave);
            var result = await dialog.ShowDialogAsync(ownerViewModel, viewModel).ConfigureAwait(false);
            return result == true ? viewModel : null;
        }

        // public static async Task<bool?> ShowDialogAsync(this IDialogService dialog, INotifyPropertyChanged ownerViewModel, IModalDialogViewModel viewModel)
        // {
        //     dialog.CheckNotNull(nameof(dialog));
        //
        //     return await Dispatcher.UIThread.InvokeAsync(() => dialog.ShowDialogAsync(ownerViewModel, viewModel));
        // }

        //private static bool IsUiThread() => Thread.CurrentThread == System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread;
    }
}
