using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.Downloads;
using HanumanInstitute.YangYouTubeDownloader.Models;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    public interface IMainViewModel
    {
        ICommand DownloadCommand { get; }
        ICommand QueryCommand { get; }
        string AudioStreamInfo { get; }
#pragma warning disable CA1056 // Uri properties should not be strings
        string DownloadUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
        bool HasDownloads { get; }
        bool DisplayDownloadInfo { get; }
        bool DisplayError { get; }
        bool IsDownloadValid { get; }
        ISelectableList<ListItem<int>> MaxQuality { get; }
        string Message { get; }
        ISelectableList<ListItem<SelectStreamFormat>> PreferredAudio { get; }
        ISelectableList<ListItem<SelectStreamFormat>> PreferredVideo { get; }
        string VideoContainer { get; }
        string VideoStreamInfo { get; }
        string VideoTitle { get; }
        ISelectableList<DownloadItem> Downloads { get; }
    }
}
