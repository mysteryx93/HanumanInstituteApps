using AppSoftware.LicenceEngine.Common;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Player432hz.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    /// <inheritdoc />
    public string GitHubFileFormat => "Player432hz-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string AppName => "432hz Player";

    /// <inheritdoc />
    public string AppDescription => "Plays music in 432hz";

    /// <inheritdoc />
    public string LicenseInfo => "The free version is fully functional. To unlock advanced settings, purchase a license for just $5 per computer to support the application development. This is a lifetime license for this app and all future updates.";

    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $5"; 

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.spiritualselftransformation.com/app-player432hz";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new[]
    {
        new KeyByteSet(1, 166, 56, 5),
        new KeyByteSet(3, 1, 94, 94),
        new KeyByteSet(5, 224, 9, 54),
        new KeyByteSet(7, 1, 228, 95)
    };
}
