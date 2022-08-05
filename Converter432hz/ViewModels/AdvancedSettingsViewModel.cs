using System.Windows.Input;
using HanumanInstitute.Common.Services.Validation;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace HanumanInstitute.Converter432hz.ViewModels;

public class AdvancedSettingsViewModel : ReactiveValidationObject, IModalDialogViewModel, ICloseable
{
    public event EventHandler? RequestClose;
    
    [Reactive]
    public EncodeSettings Settings { get; set; } = new EncodeSettings();

    public bool? DialogResult { get; set; }
    
    /// <summary>
    /// Closes the window.
    /// </summary>
    public RxCommandUnit Close => _close ??= ReactiveCommand.Create(CloseImpl);
    private RxCommandUnit? _close;
    private void CloseImpl()
    {
        if (Settings.Validate() == null)
        {
            DialogResult = true;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
