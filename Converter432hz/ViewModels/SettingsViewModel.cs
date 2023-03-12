using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Common.Services.Validation;

namespace HanumanInstitute.Converter432hz.ViewModels;

/// <inheritdoc />
public class SettingsViewModel : SettingsViewModelBase<AppSettingsData>
{
    private readonly IEnvironmentService _environment;
    
    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IEnvironmentService environment) :
        base(settingsProvider)
    {
        _environment = environment;
    }

    protected override AppSettingsData CloneSettings(AppSettingsData value) => Cloning.DeepClone(value);

    protected override bool Validate() => Settings.Validate() == null;

    /// <inheritdoc />
    protected override void RestoreDefaultImpl()
    {
        CheckForUpdateList.SelectedValue = UpdateInterval.Biweekly;
        Settings.Encode.AntiAlias = false;
        Settings.Encode.AntiAliasLength = 32;
        Settings.Encode.Speed = 1;
        Settings.Encode.AutoDetectPitch = true;
        Settings.Encode.PitchFrom = 440;
        Settings.Encode.PitchTo = 432;
        Settings.MaxThreads = Math.Min(64, _environment.ProcessorCount);
    }
}
