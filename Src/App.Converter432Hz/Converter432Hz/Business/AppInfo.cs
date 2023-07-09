using AppSoftware.LicenceEngine.Common;

namespace HanumanInstitute.Converter432Hz.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    //public string GitHubFileFormat => "Converter432Hz-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string DownloadInfoUrl => "https://github.com/mysteryx93/NaturalGroundingPlayer/releases";

    /// <inheritdoc />
    public AppIdentifier Id => AppIdentifier.Converter432Hz;

    /// <inheritdoc />
    public string AppName => "432Hz Batch Converter";

    /// <inheritdoc />
    public string AppDescription => "Converts and re-encodes music to 432Hz";

    /// <inheritdoc />
    public string LicenseInfo => "This is an open source software and provides all features for free. We run our own ads to support the development of these apps and share no data to 3rd parties. Get a lifetime app license to remove the ads and support us!";

    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $8.97"; 

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.spiritualselftransformation.com/apps?id=0";
    
    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new[]
    {
        new KeyByteSet(1, 232, 93, 227),
        new KeyByteSet(3, 115, 100, 30),
        new KeyByteSet(5, 240, 64, 6),
        new KeyByteSet(7, 230, 121, 231)
    };
}
