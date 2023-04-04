using AppSoftware.LicenceEngine.Common;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Converter432Hz.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    /// <inheritdoc />
    public string GitHubFileFormat => "Converter432Hz-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string AppName => "432Hz Batch Converter";

    /// <inheritdoc />
    public string AppDescription => "Converts and re-encodes music to 432Hz";

    /// <inheritdoc />
    public string LicenseInfo => "This is an open source software. The free version only has a single restriction: encoding entire folder hierarchies. This is a lifetime license for this app and all future updates.";

    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $16.95"; 

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
