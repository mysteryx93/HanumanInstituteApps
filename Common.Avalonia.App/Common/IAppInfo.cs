namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Provides information about the running application.
/// </summary>
public interface IAppInfo
{
    /// <summary>
    /// For updates, the file format of released files, where {0} is replaced by the version number.
    /// </summary>
    string GitHubFileFormat { get; }
    /// <summary>
    /// The name of the application.
    /// </summary>
    string AppName { get; }
    /// <summary>
    /// The description of the application. 
    /// </summary>
    string AppDescription { get; }
}
