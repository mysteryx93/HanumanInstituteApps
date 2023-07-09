using System.ComponentModel;

namespace HanumanInstitute.Apps;

/// <summary>
/// Checks for updates based on application settings.
/// </summary>
public interface IAppUpdateService
{
    /// <summary>
    /// Checks for updates based on application settings and offers to open a download link if available.
    /// </summary>
    Task CheckForUpdatesAsync(INotifyPropertyChanged owner);
}
