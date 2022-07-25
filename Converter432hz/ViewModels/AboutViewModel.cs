using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Converter432hz.ViewModels;

public class AboutViewModel : AboutViewModel<AppSettingsData>
{
    public AboutViewModel(IEnvironmentService environment, IProcessService processService,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService) :
        base(environment, processService, settings, updateService)
    {
    }
    
    public override string UpdateFileFormat => "Converter432hz-{0}_Win_x64.zip";

    public override string AppName => "432hz Batch Converter";

    public override string AppDescription => "Converts and re-encodes music to 432hz";
}
