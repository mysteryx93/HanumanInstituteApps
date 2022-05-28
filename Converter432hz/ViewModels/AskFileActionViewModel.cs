using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.Converter432hz.ViewModels;

public class AskFileActionViewModel : ReactiveObject, IModalDialogViewModel
{
    public bool? DialogResult { get; } = false;
    
    public FileExistsAction Action { get; set; }
}
