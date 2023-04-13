using AppSoftware.LicenceEngine.Common;

namespace HanumanInstitute.Player432Hz.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    // /// <inheritdoc />
    // public string GitHubFileFormat => "Player432Hz-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string DownloadInfoUrl => "https://github.com/mysteryx93/NaturalGroundingPlayer/releases";

    /// <inheritdoc />
    public AppIdentifier Id => AppIdentifier.Player432Hz;
    
    /// <inheritdoc />
    public string AppName => "432Hz Player";

    /// <inheritdoc />
    public string AppDescription => "Plays music in 432Hz";

    /// <inheritdoc />
    public string LicenseInfo => "This is an open source software. The free version has no restriction. You can get a license to support the development of these apps. This is a lifetime license for this app and all future updates.";

    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $6.95"; 

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.spiritualselftransformation.com/apps?id=1";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new[]
    {
        new KeyByteSet(1, 166, 56, 5),
        new KeyByteSet(3, 1, 94, 94),
        new KeyByteSet(5, 224, 9, 54),
        new KeyByteSet(7, 1, 228, 95)
    };
}
