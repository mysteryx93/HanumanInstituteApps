using System;
using System.ComponentModel;
using System.Windows.Input;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.Downloads;
using HanumanInstitute.YangYouTubeDownloader.Models;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        ICommand DownloadCommand { get; }
        ICommand QueryCommand { get; }
        string AudioStreamInfo { get; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "For UI binding")]
        string DownloadUrl { get; set; }
        bool HasDownloads { get; }
        bool DisplayDownloadInfo { get; }
        bool DisplayError { get; }
        bool IsDownloadValid { get; }
        ISelectableList<ListItem<int>> MaxQuality { get; }
        string Message { get; }
        ISelectableList<ListItem<StreamContainerOption>> PreferredAudio { get; }
        ISelectableList<ListItem<StreamContainerOption>> PreferredVideo { get; }
        //string VideoContainer { get; }
        string VideoStreamInfo { get; }
        string VideoTitle { get; }
        ISelectableList<DownloadItem> Downloads { get; }
    }
}
