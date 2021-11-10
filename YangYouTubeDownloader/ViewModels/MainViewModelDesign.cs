using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.Downloads;
using HanumanInstitute.YangYouTubeDownloader.Models;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    public class MainViewModelDesign : IMainViewModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand DownloadCommand => new RelayCommand(null);

        public ICommand QueryCommand => new RelayCommand(null);

        public string AudioStreamInfo => "[AudioStreamInfo]";

        public string DownloadUrl { get; set; } = "[DownloadUrl]";

        public bool HasDownloads => true;

        public bool DisplayError => false;

        public bool DisplayDownloadInfo => true;

        public bool IsDownloadValid => true;

        public ICollectionView<ListItem<int>> MaxQuality => new ListItemCollectionView<int>();

        public string Message => "Message";

        public ICollectionView<ListItem<StreamContainerOption>> PreferredAudio => new ListItemCollectionView<StreamContainerOption>();

        public ICollectionView<ListItem<StreamContainerOption>> PreferredVideo => new ListItemCollectionView<StreamContainerOption>();

        public string VideoStreamInfo => "[VideoStreamInfo]";

        public string VideoTitle => "[VideoTitle]";

        public ObservableCollection<DownloadItem> Downloads => new ObservableCollection<DownloadItem>();
    }
}
