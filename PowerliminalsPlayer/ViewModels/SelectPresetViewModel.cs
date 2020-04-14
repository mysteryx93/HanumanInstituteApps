using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HanumanInstitute.PowerliminalsPlayer.Business;
using HanumanInstitute.CommonWpfApp;
using GalaSoft.MvvmLight;
using MvvmDialogs;
using PropertyChanged;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels
{
    [AddINotifyPropertyChangedInterface()]
    public class SelectPresetViewModel : ViewModelBase, IModalDialogViewModel
    {
        protected readonly AppSettingsProvider appSettings;
        public AppSettingsFile AppData => appSettings?.Current;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public SelectPresetViewModel(AppSettingsProvider appSettings)
        {
            this.appSettings = appSettings;
        }

        public event EventHandler RequestClose;

        public bool? DialogResult { get; set; } = false;

        public string PresetName { get; set; } = "New Preset";

        public PresetItem SelectedItem { get; set; }
        public bool ModeSave { get; set; } = true;

        public SelectPresetViewModel Load(bool modelSave)
        {
            ModeSave = modelSave;
            if (!ModeSave)
            {
                SelectedItem = AppData.Presets.FirstOrDefault();
            }
            return this;
        }

        private void OnSelectedItemChanged()
        {
            if (SelectedItem != null)
            {
                PresetName = SelectedItem.Name;
            }
        }

        private ICommand confirmCommand;
        public ICommand ConfirmCommand => CommandHelper.InitCommand(ref confirmCommand, OnConfirm, () => CanConfirm);
        private bool CanConfirm => ModeSave ? !string.IsNullOrWhiteSpace(PresetName) : SelectedItem != null;
        private void OnConfirm()
        {
            if (CanConfirm)
            {
                DialogResult = true;
                RequestClose?.Invoke(this, new EventArgs());
            }
        }

        private ICommand deleteCommand;
        public ICommand DeleteCommand => CommandHelper.InitCommand(ref deleteCommand, OnDelete, () => CanDelete);
        private bool CanDelete => SelectedItem != null;
        private void OnDelete()
        {
            if (CanDelete)
            {
                AppData.Presets.Remove(SelectedItem);
            }
        }

        public void PresetList_MouseDoubleClick(MouseButtonEventArgs e)
        {
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is PresetItem && e.LeftButton == MouseButtonState.Pressed)
            {
                OnConfirm();
            }
        }
    }
}
