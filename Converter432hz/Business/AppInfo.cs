using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Converter432hz.Business;

/// <inheritdoc />
public class AppInfo : IAppInfo
{
    /// <inheritdoc />
    public string GitHubFileFormat => "Converter432hz-{0}_Win_x64.zip";

    /// <inheritdoc />
    public string AppName => "432hz Batch Converter";

    /// <inheritdoc />
    public string AppDescription => "Converts and re-encodes music to 432hz";
}
