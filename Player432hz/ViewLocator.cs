using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MvvmDialogs.Avalonia;
using AboutView = HanumanInstitute.Player432hz.Views.AboutView;

namespace HanumanInstitute.Player432hz;

/// <summary>
/// Maps view models to views.
/// </summary>
public class ViewLocator : ViewLocatorBase
{
    /// <inheritdoc />
    protected override string GetViewName(object viewModel) => viewModel.GetType().FullName!.Replace("ViewModel", "View");
}
