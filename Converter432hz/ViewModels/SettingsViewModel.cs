using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Common.Services.Validation;

namespace HanumanInstitute.Converter432hz.ViewModels;

/// <inheritdoc />
public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    private readonly IEnvironmentService _environment;
    
    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme, IEnvironmentService environment) :
        base(settingsProvider, fluentTheme)
    {
        _environment = environment;
    }

    protected override AppSettingsData CloneSettings(AppSettingsData value) => Cloning.DeepClone(value);

    protected override bool Validate() => Settings.Validate() == null;

    /// <inheritdoc />
    protected override void RestoreDefaultImpl()
    {
        Settings.Encode.AntiAlias = false;
        Settings.Encode.AntiAliasLength = 32;
        Settings.Encode.Speed = 1;
        Settings.Encode.AutoDetectPitch = true;
        Settings.Encode.PitchFrom = 440;
        Settings.Encode.PitchTo = 432;
        Settings.MaxThreads = Math.Min(64, _environment.ProcessorCount);
    }
}
