using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using HanumanInstitute.PowerliminalsPlayer.Views;

namespace HanumanInstitute.PowerliminalsPlayer;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    private MainViewModel _mainViewModel = default!;

    protected override Task<INotifyPropertyChanged?> InitViewModelAsync()
    {
        _mainViewModel = ViewModelLocator.Main;
        _mainViewModel.LoadSettings();
        return Task.FromResult<INotifyPropertyChanged?>(_mainViewModel);
    }

    protected override Task InitCompleted() => _mainViewModel.PromptFixPathsAsync();
}
