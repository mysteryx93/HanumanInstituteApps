using System;
using System.ComponentModel;
using MvvmDialogs.DialogTypeLocators;

namespace HanumanInstitute.CommonWpfApp
{
    /// <summary>
    /// MvvmDialog type locator that assumes ViewModels.MainViewModel to Views.MainView
    /// </summary>
    public class AppDialogTypeLocator : IDialogTypeLocator
    {
        public Type Locate(INotifyPropertyChanged viewModel)
        {
            if (viewModel == null) { throw new ArgumentNullException(nameof(viewModel)); }

            var viewModelType = viewModel.GetType();
            var dialogTypeName = viewModelType.FullName;

            // Replace namespace from ViewModels to Views.
            dialogTypeName = dialogTypeName.Replace("ViewModels.", "Views.");

            // Replace sufix from ViewModel to View.
            const string OldSufix = "ViewModel";
            const string NewSufix = "View";
            if (dialogTypeName.EndsWith(OldSufix, StringComparison.InvariantCulture))
            {
                dialogTypeName = dialogTypeName.Substring(0, dialogTypeName.Length - OldSufix.Length) + NewSufix;
            }

            return viewModelType.Assembly.GetType(dialogTypeName);
        }
    }
}
