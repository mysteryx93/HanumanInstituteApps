using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonWpfApp;
using MvvmWizard.Classes;

namespace HanumanInstitute.MediaMuxer.ViewModels
{
    /// <summary>
    /// Represents the first page of the wizard.
    /// </summary>
    public class WizardMainViewModel : StepViewModelBase
    {
        private WizardSection _currentSection;

        public override Task OnTransitedTo(TransitionContext transitionContext)
        {
            _currentSection = WizardSection.Main;

            return base.OnTransitedTo(transitionContext);
        }

        public override Task OnTransitedFrom(TransitionContext transitionContext)
        {
            var index = transitionContext.StepIndices.First(x => x.Value == _currentSection.ToString()).Key;
            transitionContext.TransitToStep = index;

            return base.OnTransitedFrom(transitionContext);
        }


        public ICommand MuxeCommand => CommandHelper.InitCommand(ref _muxeCommand, OnMuxe, () => true);
        private RelayCommand? _muxeCommand;
        private void OnMuxe()
        {
            _currentSection = WizardSection.Muxe;
            TransitionController.NextStepCommand.Execute(null);
        }


        public ICommand MergeCommand => CommandHelper.InitCommand(ref _mergeCommand, OnMerge, () => true);
        private RelayCommand? _mergeCommand;
        private void OnMerge()
        {
            _currentSection = WizardSection.Merge;
            TransitionController.NextStepCommand.Execute(null);
        }


        private enum WizardSection
        {
            Main,
            Muxe,
            Merge,
            Working
        }
    }
}
