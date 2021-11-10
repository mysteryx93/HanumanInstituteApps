using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using HanumanInstitute.CommonWpfApp;
using MvvmWizard.Classes;
using System.Threading.Tasks;
using System.IO;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.MediaMuxer.ViewModels
{
    public class WizardMuxe2ViewModel : StepViewModelBase
    {
        private IDialogService _dialogService;
        private IMediaMuxer _mediaMuxer;

        public WizardMuxe2ViewModel(IDialogService dialogService, IMediaMuxer mediaMuxer)
        {
            _dialogService = dialogService;
            _mediaMuxer = mediaMuxer;
        }

        public string ContainerExtension { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;

        public ICommand BrowseCommand => CommandHelper.InitCommand(ref _browseCommand, OnBrowse, () => true);
        private RelayCommand? _browseCommand;
        private void OnBrowse()
        {
            var ext = ContainerExtension.Trim();
            var settings = new SaveFileDialogSettings()
            {
                Filter = string.IsNullOrWhiteSpace(ContainerExtension) ? string.Empty : $"{ext}|*.{ext}",
                CheckPathExists = true
            };
            if (_dialogService.ShowSaveFileDialog(this, settings) == true)
            {
                OutputFile = settings.FileName;
            }
        }

        public override Task OnTransitedFrom(TransitionContext transitionContext)
        {
            if (transitionContext.TransitToStep > transitionContext.TransitedFromStep)
            {
                bool success = false;
                var fileName = OutputFile.Trim();
                if (!string.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        File.Create(fileName).Close();
                        File.Delete(fileName);
                        transitionContext.SharedContext["OutputFile"] = OutputFile;
                        success = true;
                    }
                    catch { }
                }

                if (!success)
                {
                    _dialogService.ShowMessageBox(this, "Please enter a valid output file name.", "Validation");
                    transitionContext.AbortTransition = true;
                }
            }
            return base.OnTransitedFrom(transitionContext);
        }
    }
}
