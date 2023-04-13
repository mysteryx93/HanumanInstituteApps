using AppSoftware.LicenceEngine.Common;
using HanumanInstitute.Apps;

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
    public string LicenseInfo => "This is an open source software. The free version has no restriction. You can get a license to support the development of these apps. This is a lifetime license for this app and all future updates.";

    /// <inheritdoc />
    public string BuyLicenseText => "Get license for $2.95"; 

    /// <inheritdoc />
    public string BuyLicenseUrl => "https://store.spiritualselftransformation.com/app-yangdownloader?id=3";

    /// <inheritdoc />
    public KeyByteSet[] KeyByteSets => new[]
    {
        new KeyByteSet(1, 146, 133, 42),
        new KeyByteSet(3, 160, 44, 218),
        new KeyByteSet(5, 211, 50, 179),
        new KeyByteSet(7, 230, 202, 203)
    };
}
