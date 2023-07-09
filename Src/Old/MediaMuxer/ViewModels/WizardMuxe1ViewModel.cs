using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.MediaMuxer.Models;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmWizard.Classes;

namespace HanumanInstitute.MediaMuxer.ViewModels
{
    public class WizardMuxe1ViewModel : StepViewModelBase
    {
        private readonly DialogService _dialogService;
        private readonly IMediaInfoReader _mediaInfo;
        private readonly IFileSystemService _fileSystem;

        public WizardMuxe1ViewModel(DialogService dialogService, IMediaInfoReader mediaInfo, IFileSystemService fileSystem)
        {
            _dialogService = dialogService;
            _mediaInfo = mediaInfo;
            _fileSystem = fileSystem;
        }

        public ListItemCollectionView<string> Files { get; private set; } = new ListItemCollectionView<string>();
        public ListItemCollectionView<UIFileStream> FileStreams { get; private set; } = new ListItemCollectionView<UIFileStream>();


        public ICommand AddFileCommand => CommandHelper.InitCommand(ref _addFileCommand, OnAddFile, () => true);
        private RelayCommand? _addFileCommand;
        private void OnAddFile()
        {
            var added = false;
            var settings = new OpenFileDialogSettings()
            {
                Multiselect = true,
                CheckFileExists = true
            };
            if (_dialogService.ShowOpenFileDialog(this, settings) == true)
            {
                foreach (var file in settings.FileNames)
                {
                    if (!Files.Source.Any(x => x.Value == file))
                    {
                        var fileInfo = _mediaInfo.GetFileInfo(file, new ProcessOptionsEncoder(ProcessDisplayMode.None)).FileStreams;
                        if (fileInfo?.Count > 0)
                        {
                            var fileName = _fileSystem.Path.GetFileName(file);
                            Files.Add(file, fileName);
                            foreach (var item in fileInfo)
                            {
                                FileStreams.Add(new UIFileStream(item, file));
                                added = true;
                            }
                        }
                    }
                }
            }

            if (added == false)
            {
                _dialogService.ShowMessageBox(this, "No video or audio stream found in file.", "Validation", icon: MessageBoxImage.Error);
            }
        }

        public override Task OnTransitedFrom(TransitionContext transitionContext)
        {
            if (transitionContext.TransitToStep > transitionContext.TransitedFromStep)
            {
                if (!FileStreams.Any(x => x.Value.IsChecked))
                {
                    transitionContext.AbortTransition = true;
                    _dialogService.ShowMessageBox(this, "No stream selected.", "Validation", icon: MessageBoxImage.Error);
                }
                else
                {
                    transitionContext.SharedContext["FileStreams"] = FileStreams.Where(x => x.Value.IsChecked);
                }
            }

            return base.OnTransitedFrom(transitionContext);
        }
    }
}
