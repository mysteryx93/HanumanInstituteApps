using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.NaturalGroundingPlayer.ViewModels;

namespace HanumanInstitute.NaturalGroundingPlayer;

/// <summary>
/// Maps view models to views.
/// </summary>
public class ViewLocator : ViewLocatorBase
{
    /// <inheritdoc />
    protected override string GetViewName(object viewModel) => viewModel.GetType().FullName!.Replace("ViewModel", "View");
}
