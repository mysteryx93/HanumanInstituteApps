using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HanumanInstitute.Common.Services;

public interface IPathFixer
{
    Task ScanAndFixFoldersAsync(INotifyPropertyChanged owner, IList<string> folders);
}
