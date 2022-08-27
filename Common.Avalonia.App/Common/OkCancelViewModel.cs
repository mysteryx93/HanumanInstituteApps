using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.Common.Avalonia.App;

public abstract class OkCancelViewModel : ReactiveObject, IModalDialogViewModel, ICloseable
{
    /// <inheritdoc />
    public event EventHandler? RequestClose;

    /// <inheritdoc />
    public bool? DialogResult { get; protected set; }

    /// <summary>
    /// Validates the data before saving.
    /// </summary>
    /// <returns>True if data is valid and ready to save, otherwise false to cancel the save.</returns>
    protected virtual bool Validate() => true;

    /// <summary>
    /// Saves the settings.
    /// </summary>
    /// <returns>True if saving was successful, otherwise false.</returns>
    protected abstract bool SaveSettings();
    
    /// <summary>
    /// Applies changes without closing the window.
    /// </summary>
    public RxCommandUnit Apply => _apply ??= ReactiveCommand.Create(ApplyImpl);
    private RxCommandUnit? _apply;
    private void ApplyImpl()
    {
        if (Validate())
        {
            SaveSettings();  
        }
    } 

    /// <summary>
    /// Saves changes and closes the window.
    /// </summary>
    public RxCommandUnit Ok => _ok ??= ReactiveCommand.Create(OkImpl);
    private RxCommandUnit? _ok;
    private void OkImpl()
    {
        if (Validate() && SaveSettings())
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Closes the window without saving.
    /// </summary>
    public RxCommandUnit Cancel => _cancel ??= ReactiveCommand.Create(CancelImpl);
    private RxCommandUnit? _cancel;
    private void CancelImpl() => RequestClose?.Invoke(this, EventArgs.Empty);
}
