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
}
