using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.PowerliminalsPlayer.Views;
using MvvmDialogs;
using MvvmDialogs.Avalonia;
using ReactiveUI;

namespace HanumanInstitute.PowerliminalsPlayer;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override Task<INotifyPropertyChanged?> InitViewModelAsync()
    {
        var result = ViewModelLocator.Main;
        result.LoadSettings();
        return Task.FromResult<INotifyPropertyChanged?>(result);
    }

    protected override async Task InitCompleted()
    {
        // var dialogService = (IDialogService)Splat.Locator.Current.GetService(typeof(IDialogService))!;
        // var owner = (INotifyPropertyChanged)DesktopLifetime!.MainWindow.DataContext!;
        // await Task.Delay(1).ConfigureAwait(true);
        // await dialogService.ShowMessageBoxAsync(owner, "Loaded!").ConfigureAwait(true);
    }

    // public override void OnFrameworkInitializationCompleted()
    // {
    //     GC.KeepAlive(typeof(DialogService));
    //     if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //     {
    //         desktop.MainWindow = new MainView { DataContext = ViewModelLocator.Main };
    //     }
    //
    //     base.OnFrameworkInitializationCompleted();
    // }
}
