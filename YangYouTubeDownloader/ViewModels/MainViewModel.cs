using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanumanInstitute.Downloads;
using GalaSoft.MvvmLight;
using PropertyChanged;
using HanumanInstitute.CommonWpf;

namespace HanumanInstitute.YangYouTubeDownloader.ViewModels
{
    [AddINotifyPropertyChangedInterface()]
    public class MainViewModel : ViewModelBase
    {
        private IDownloadManager downloadManager;
        public VideoInfo Info;
        public string VideoUrl;
        DownloadOptions Options;

        public MainViewModel(IDownloadManager downloadManager)
        {
            this.downloadManager = downloadManager;
        }

        public ObservableCollection<DownloadTaskInfo> ActiveDownloads => downloadManager.DownloadsList;

        public bool IsGridInfoVisible { get; private set; } = true;

        public bool IsDownloadsViewVisible { get; private set; } = true;


        private ObservableCollection<ListItem<int>> maxDownloadQualityList;
        public ObservableCollection<ListItem<int>> MaxDownloadQualityList {
            get {
                if (maxDownloadQualityList == null)
                {
                    var newList = new ObservableCollection<ListItem<int>>();
                    newList.Add(new ListItem<int>("Max", 0));
                    newList.Add(new ListItem<int>("1080p", 1080));
                    newList.Add(new ListItem<int>("720p", 720));
                    newList.Add(new ListItem<int>("480p", 480));
                    newList.Add(new ListItem<int>("360p", 360));
                    newList.Add(new ListItem<int>("240p", 240));
                    maxDownloadQualityList = newList;
                }
                return maxDownloadQualityList;
            }
        }

        public int MaxDownloadQualitySelectedIndex { get; set; }

    }
}
