namespace HanumanInstitute.Services;

/// <summary>
/// Checks GitHub for the latest released version.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Gets the repo in which to look for releases.
    /// </summary>
    string GitRepo { get; set; }

    /// <summary>
    /// Gets the link to view the latest version.
    /// </summary>
    string GetDownloadInfoLink();
    
    /// <summary>
    /// Gets the download file format to look for in each releases. {0} will be replaced by the version number without 'v'.
    /// </summary>
    string FileFormat { get; set; }

    /// <summary>
    /// Returns the GitHub release atom feed based on GitRepo. 
    /// </summary>
    string FeedUrl { get; }
    
    /// <summary>
    /// Returns the latest version of the application released on GitHub. 
    /// </summary>
    /// <returns>The latest version from GitHub.</returns>
    /// <exception cref="ArgumentException">FileFormat was not set.</exception>
    Version? GetLatestVersion();

    /// <summary>
    /// Returns the latest version of the application released on GitHub. 
    /// </summary>
    /// <returns>The latest version from GitHub.</returns>
    /// <exception cref="ArgumentException">FileFormat was not set.</exception>
    Task<Version?> GetLatestVersionAsync();
}
