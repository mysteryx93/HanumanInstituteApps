// using System;
// using System.ComponentModel;
// using System.Globalization;
// using MvvmDialogs.DialogTypeLocators;
// using HanumanInstitute.CommonServices;
//
// namespace HanumanInstitute.CommonAvaloniaApp
// {
//     /// <summary>
//     /// MvvmDialog type locator that assumes ViewModels.MainViewModel to Views.MainView
//     /// </summary>
//     public class AppDialogTypeLocator : IDialogTypeLocator
//     {
//         public Type Locate(INotifyPropertyChanged viewModel)
//         {
//             viewModel.CheckNotNull(nameof(viewModel));
//
//             var viewModelType = viewModel.GetType();
//             var viewModelTypeName = viewModelType.FullName ?? string.Empty;
//
//             // Replace namespace from ViewModels to Views.
//             var viewTypeName = viewModelTypeName.Replace("ViewModels.", "Views.");
//
//             // Replace sufix from ViewModel to View.
//             const string OldSufix = "ViewModel";
//             const string NewSufix = "View";
//             if (viewTypeName.EndsWith(OldSufix, StringComparison.InvariantCulture))
//             {
//                 viewTypeName = viewTypeName.Substring(0, viewTypeName.Length - OldSufix.Length) + NewSufix;
//             }
//
//             return viewModelType.Assembly.GetType(viewTypeName) ??
//                 throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.DialogLocatorViewNotFound, viewTypeName, viewModelTypeName));
//         }
//     }
// }
