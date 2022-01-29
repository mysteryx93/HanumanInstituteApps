using System.ComponentModel;
using System.Threading.Tasks;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

public interface IFolderPathFixer
{
    Task PromptFixPathsAsync(INotifyPropertyChanged owner);
}
