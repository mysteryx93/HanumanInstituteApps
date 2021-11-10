using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.PowerliminalsPlayer.Models;
using MvvmDialogs;
using PropertyChanged;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels
{
    [AddINotifyPropertyChangedInterface()]
    public class SelectPresetViewModel : ViewModelBase, IModalDialogViewModel
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

        public string PresetName { get; set; } = "New Preset";

        public PresetItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                if (value != null)
                {
                    PresetName = value.Name;
                }
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }
        private PresetItem? _selectedItem;

        public bool ModeSave { get; set; } = true;

        public SelectPresetViewModel Load(bool modeSave)
        {
            ModeSave = modeSave;
            if (!ModeSave)
            {
                SelectedItem = AppData.Presets.FirstOrDefault();
            }
            return this;
        }

        public ICommand ConfirmCommand => CommandHelper.InitCommand(ref _confirmCommand, OnConfirm, () => CanConfirm);
        private RelayCommand? _confirmCommand;
        private bool CanConfirm => ModeSave ? !string.IsNullOrWhiteSpace(PresetName) : SelectedItem != null;
        private void OnConfirm()
        {
            if (CanConfirm)
            {
                DialogResult = true;
                RequestClose?.Invoke(this, new EventArgs());
            }
        }

        public ICommand DeleteCommand => CommandHelper.InitCommand(ref _deleteCommand, OnDelete, () => CanDelete);
        private RelayCommand? _deleteCommand;
        private bool CanDelete => SelectedItem != null;
        private void OnDelete()
        {
            if (CanDelete)
            {
                AppData.Presets.Remove(SelectedItem!);
            }
        }

        public void OnPresetListMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.CheckNotNull(nameof(e));
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is PresetItem && e.LeftButton == MouseButtonState.Pressed)
            {
                OnConfirm();
            }
        }
    }
}
