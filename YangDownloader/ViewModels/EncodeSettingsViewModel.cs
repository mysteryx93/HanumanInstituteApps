using System.Windows.Input;
using HanumanInstitute.BassAudio;
using HanumanInstitute.Common.Services.Validation;
using ReactiveUI;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class EncodeSettingsViewModel : ReactiveObject, IModalDialogViewModel, ICloseable
{
    public event EventHandler? RequestClose;
    
    [Reactive]
    public EncodeSettings Settings { get; set; } = new EncodeSettings();

    public bool? DialogResult { get; set; }
    
    /// <summary>
    /// Closes the window.
    /// </summary>
    public ICommand Close => _close ??= ReactiveCommand.Create(CloseImpl);
    private ICommand? _close;
    private void CloseImpl()
    {
        if (Settings.Validate() == null)
        {
            DialogResult = true;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
