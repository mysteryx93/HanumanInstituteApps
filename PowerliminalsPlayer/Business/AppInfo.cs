using AppSoftware.LicenceEngine.Common;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    /// <inheritdoc />
    public string GitHubFileFormat => "PowerliminalsPlayer-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string AppName => "Powerliminals Player";

    /// <inheritdoc />
    public string AppDescription => "Plays multiple audios simultaneously at varying speeds";

    /// <inheritdoc />
    public string LicenseInfo =>
        "Play many audios at low volume with high energetic frequencies side-by-side 24/7 to stabilize the energy of your environment. The same audio can be played 5 times at different speeds. To make full use of this software, we recommend our Powerliminals Pack for a series of silent audios with very high frequencies.";

    /// <inheritdoc />
    public string BuyLicenseText => "Get the Powerliminals Pack"; 

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://www.spiritualselftransformation.com/powerliminals-nonrivalry";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new KeyByteSet[]
    {
        
    };
}
