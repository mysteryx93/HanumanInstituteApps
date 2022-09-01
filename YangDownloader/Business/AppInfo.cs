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
}
