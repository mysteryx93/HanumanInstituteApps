using AppSoftware.LicenceEngine.Common;

namespace HanumanInstitute.YangDownloader.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    // /// <inheritdoc />
    // public string GitHubFileFormat => "YangDownloader-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string DownloadInfoUrl => "https://github.com/mysteryx93/NaturalGroundingPlayer/releases";

    /// <inheritdoc />
    public AppIdentifier Id => AppIdentifier.YangDownloader;
    
    /// <inheritdoc />
    public string AppName => "Yang YouTube Downloader";

    /// <inheritdoc />
    public string AppDescription => "Downloads best-quality audio and video from YouTube.";
    
    /// <inheritdoc />
    public string LicenseInfo => "This is an open source software and provides all features for free. We run our own ads to support the development of these apps and share no data to 3rd parties. Get a lifetime app license to remove the ads and support us!";

    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $2.97";

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.hanumaninstitute.com/apps?id=3";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new[]
    {
        new KeyByteSet(1, 146, 133, 42),
        new KeyByteSet(3, 160, 44, 218),
        new KeyByteSet(5, 211, 50, 179),
        new KeyByteSet(7, 230, 202, 203)
    };
}
