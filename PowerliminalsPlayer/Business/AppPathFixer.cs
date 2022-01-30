using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Models;
using MvvmDialogs;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <inheritdoc />
public class AppPathFixer : PathFixerBase
{
    private readonly ISettingsProvider<AppSettingsData> _settings;

    /// <inheritdoc />
    public AppPathFixer(IFileSystemService fileSystem, IDialogService dialogService, ISettingsProvider<AppSettingsData> settings) :
        base(fileSystem, dialogService)
    {
        _settings = settings;
    }
    
    /// <inheritdoc />
    protected override void Apply(string replaceOld, string replaceNew)
    {
        for (var i = 0; i < _settings.Value.Folders.Count; i++)
        {
            _settings.Value.Folders[i] = Replace(_settings.Value.Folders[i], replaceOld, replaceNew);
        }
        foreach (var preset in _settings.Value.Presets)
        {
            foreach (var presetItem in preset.Files)
            {
                presetItem.FullPath = Replace(presetItem.FullPath, replaceOld, replaceNew);
            }
        }
    }
}
