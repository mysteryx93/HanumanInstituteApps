using System.ComponentModel;

namespace HanumanInstitute.Common.Avalonia.App;

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
