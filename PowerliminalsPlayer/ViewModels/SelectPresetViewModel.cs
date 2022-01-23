using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using HanumanInstitute.Common.Avalonia;
using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Models;
using MvvmDialogs;
using ReactiveUI;
using ICloseable = MvvmDialogs.ICloseable;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

public class SelectPresetViewModel : ViewModelBase, IModalDialogViewModel, ICloseable
{
    private readonly ISettingsProvider<AppSettingsData> _appSettings;
    public AppSettingsData AppData => _appSettings.Value;

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    public SelectPresetViewModel(ISettingsProvider<AppSettingsData> appSettings)
    {
        _appSettings = appSettings.CheckNotNull(nameof(appSettings));
    }

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; set; } = false;

    public string PresetName
    {
        get => _presetName;
        set => this.RaiseAndSetIfChanged(ref _presetName, value);
    }
    private string _presetName = string.Empty;

    public PresetItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedItem, value);
            if (value != null)
            {
                PresetName = value.Name;
            }
        }
    }
    private PresetItem? _selectedItem;

    public bool ModeSave
    {
        get => _modeSave;
        set => this.RaiseAndSetIfChanged(ref _modeSave, value);
    }
    private bool _modeSave = true;

    public SelectPresetViewModel Load(bool modeSave)
    {
        ModeSave = modeSave;
        if (!ModeSave)
        {
            SelectedItem = AppData.Presets.FirstOrDefault();
        }
        else
        {
            PresetName = "New Preset";
        }

        return this;
    }

    public ICommand ConfirmCommand => _confirmCommand ??= ReactiveCommand.Create(OnConfirm,
        this.WhenAnyValue(x => x.ModeSave, x => x.PresetName, x => x.SelectedItem, (modeSave, presetName, selectedItem) =>
            modeSave ? !string.IsNullOrWhiteSpace(presetName) : selectedItem != null));
    private ICommand? _confirmCommand;
    private void OnConfirm()
    {
        if (!ConfirmCommand.CanExecute(null)) return;
        
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public ICommand CancelCommand => _cancelCommand ??= ReactiveCommand.Create(OnCancel);
    private ICommand? _cancelCommand;
    private void OnCancel() => RequestClose?.Invoke(this, EventArgs.Empty);

    public ICommand DeleteCommand => _deleteCommand ??= ReactiveCommand.Create(OnDelete,
        this.WhenAnyValue(x => x.SelectedItem).Select(x => x != null));
    private ICommand? _deleteCommand;
    private void OnDelete()
    {
        if (SelectedItem != null)
        {
            AppData.Presets.Remove(SelectedItem);
        }
    }
}