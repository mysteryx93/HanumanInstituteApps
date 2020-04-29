using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.Downloads;
using GalaSoft.MvvmLight.CommandWpf;
using System.Globalization;
using System.Collections.ObjectModel;
using HanumanInstitute.YangYouTubeDownloader.Models;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    public class MainViewModelDesign : IMainViewModel
    {
        public ICommand DownloadCommand => new RelayCommand(null);

        public ICommand QueryCommand => new RelayCommand(null);

        public string AudioStreamInfo => "[AudioStreamInfo]";

#pragma warning disable CA1056 // Uri properties should not be strings
        public string DownloadUrl { get; set; } = "[DownloadUrl]";
#pragma warning restore CA1056 // Uri properties should not be strings

        public bool HasDownloads => true;

        public bool DisplayError => false;

        public bool DisplayDownloadInfo => true;

        public bool IsDownloadValid => true;

        public ISelectableList<ListItem<int>> MaxQuality => new SelectableList<ListItem<int>>();

        public string Message => "Message";

        public ISelectableList<ListItem<SelectStreamFormat>> PreferredAudio => new SelectableList<ListItem<SelectStreamFormat>>();

        public ISelectableList<ListItem<SelectStreamFormat>> PreferredVideo => new SelectableList<ListItem<SelectStreamFormat>>();

        public string VideoContainer => "VideoContainer";

        public string VideoStreamInfo => "[VideoStreamInfo]";

        public string VideoTitle => "[VideoTitle]";

        public ISelectableList<DownloadItem> Downloads => new SelectableList<DownloadItem>();
    }
}
