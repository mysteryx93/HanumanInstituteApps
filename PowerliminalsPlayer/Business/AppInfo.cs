using AppSoftware.LicenceEngine.Common;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    // /// <inheritdoc />
    // public string GitHubFileFormat => "PowerliminalsPlayer-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string DownloadInfoUrl => "https://github.com/mysteryx93/NaturalGroundingPlayer/releases";

    /// <inheritdoc />
    public AppIdentifier Id => AppIdentifier.PowerliminalsPlayer;
    
    /// <inheritdoc />
    public string AppName => "Powerliminals Player";

    /// <inheritdoc />
    public string AppDescription => "Plays multiple audios simultaneously at varying speeds";

    /// <inheritdoc />
    public string LicenseInfo => "This is an open source software and provides all features for free. We run our own ads to support the development of these apps and share no data to 3rd parties. Get a lifetime app license to remove the ads and support us!";
    
    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $5.97";

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.spiritualselftransformation.com/apps?id=2";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new[]
    {
        new KeyByteSet(1, 25, 37, 164),
        new KeyByteSet(3, 50, 226, 55),
        new KeyByteSet(5, 126, 127, 246),
        new KeyByteSet(7, 126, 113, 86)
    };
}
