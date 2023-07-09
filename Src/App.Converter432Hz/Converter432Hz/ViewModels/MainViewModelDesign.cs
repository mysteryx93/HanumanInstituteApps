using HanumanInstitute.Apps.AdRotator;
using HanumanInstitute.MvvmDialogs;
using Splat;

namespace HanumanInstitute.Converter432Hz.ViewModels;

public class MainViewModelDesign : MainViewModel
{
    public MainViewModelDesign() :
        base(Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!, 
            Locator.Current.GetService<IAppUpdateService>()!, 
            Locator.Current.GetService<IEncoderService>()!,
            Locator.Current.GetService<IDialogService>()!, 
            Locator.Current.GetService<IFileSystemService>()!,
            Locator.Current.GetService<IFileLocator>()!, 
            Locator.Current.GetService<IAppPathService>()!, 
            Locator.Current.GetService<IEnvironmentService>()!,
            Locator.Current.GetService<IPitchDetector>()!,
            Locator.Current.GetService<AdRotatorViewModel>()!)
    {
        Encoder.ProcessingFiles.Add(new ProcessingItem("", "Short Item"));
        Encoder.ProcessingFiles.Add(new ProcessingItem("", "Long Long Long Long Long Long Long Long Long Long Long Long "));
    }
}
