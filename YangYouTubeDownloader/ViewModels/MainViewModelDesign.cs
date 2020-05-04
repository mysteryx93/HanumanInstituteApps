using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.Downloads;
using HanumanInstitute.YangYouTubeDownloader.Models;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    public class MainViewModelDesign : IMainViewModel
    {
        public ICommand DownloadCommand => new RelayCommand(null);

        public ICommand QueryCommand => new RelayCommand(null);

        public string AudioStreamInfo => "[AudioStreamInfo]";

        public string DownloadUrl { get; set; } = "[DownloadUrl]";

        public bool HasDownloads => true;

        public bool DisplayError => false;

        public bool DisplayDownloadInfo => true;

        public bool IsDownloadValid => true;

        public ISelectableList<ListItem<int>> MaxQuality => new SelectableList<ListItem<int>>();

        public string Message => "Message";

        public ISelectableList<ListItem<StreamContainerOption>> PreferredAudio => new SelectableList<ListItem<StreamContainerOption>>();

        public ISelectableList<ListItem<StreamContainerOption>> PreferredVideo => new SelectableList<ListItem<StreamContainerOption>>();

        public string VideoStreamInfo => "[VideoStreamInfo]";

        public string VideoTitle => "[VideoTitle]";

        public ISelectableList<DownloadItem> Downloads => new SelectableList<DownloadItem>();
    }
}
