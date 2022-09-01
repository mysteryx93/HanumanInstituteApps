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
}
