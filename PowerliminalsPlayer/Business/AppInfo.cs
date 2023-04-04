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
    public string LicenseInfo => "This is an open source software. The free version has no restriction. You can get a license to support the development of these apps. This is a lifetime license for this app and all future updates.";
    
    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $6.95";

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.spiritualselftransformation.com/apps?id=3";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new KeyByteSet[]
    {
        
    };
}
