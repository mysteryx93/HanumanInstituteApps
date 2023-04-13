using AppSoftware.LicenceEngine.Common;

namespace HanumanInstitute.Apps;

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
    /// <summary>
    /// Detailed license information to display before purchase.
    /// </summary>
    string LicenseInfo { get; }
    /// <summary>
    /// Link to purchase a license.
    /// </summary>
    string BuyLicenseUrl { get; }
    /// <summary>
    /// Text to display with the purchase link.
    /// </summary>
    string BuyLicenseText { get; }
    /// <summary>
    /// The byte sets to validate with AppSoftware.LicenceEngine.KeyVerification. It will not include all byte sets defined during generation.  
    /// </summary>
    public KeyByteSet[] KeyByteSets { get; }
    /// <summary>
    /// The amount of byte sets defined during generation.
    /// </summary>
    public int TotalKeyByteSets => 7;
}
