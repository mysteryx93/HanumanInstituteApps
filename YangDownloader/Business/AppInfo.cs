using AppSoftware.LicenceEngine.Common;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.YangDownloader.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    /// <inheritdoc />
    public string GitHubFileFormat => "YangDownloader-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string AppName => "Yang YouTube Downloader";

    /// <inheritdoc />
    public string AppDescription => "Downloads best-quality audio and video from YouTube.";
    
    /// <inheritdoc />
    public string LicenseInfo => "The free version is fully functional. To unlock advanced settings, purchase a license for just $5 per computer to support the application development. This is a lifetime license for this app and all future updates.";

    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $5"; 

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.spiritualselftransformation.com/app-yangdownloader";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new KeyByteSet[]
    {
        
    };
}
