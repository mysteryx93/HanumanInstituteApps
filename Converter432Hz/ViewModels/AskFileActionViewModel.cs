using System.Windows.Input;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.Converter432Hz.ViewModels;

public class AskFileActionViewModel : ReactiveObject, IModalDialogViewModel, ICloseable
{
    public event EventHandler? RequestClose;

    [Reactive] public string FilePath { get; set; } = string.Empty;
    
    [Reactive] public bool? DialogResult { get; set; } = false;

    public ListItemCollectionView<FileExistsAction> Items { get; } = new()
    {
        { FileExistsAction.Skip, "Skip" },
        { FileExistsAction.Overwrite, "Overwrite" },
        { FileExistsAction.Rename, "Rename" },
        { FileExistsAction.Cancel, "Cancel" }
    };

    public bool ApplyToAll { get; set; }

    public RxCommandUnit Ok => _ok ??= ReactiveCommand.Create(OkImpl, 
        this.WhenAnyValue(x => x.Items.SelectedValue, x => x != FileExistsAction.Ask));
    private RxCommandUnit? _ok;
    private void OkImpl()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public FileExistsAction Action => DialogResult == true ? Items.SelectedValue : FileExistsAction.Skip;
}
