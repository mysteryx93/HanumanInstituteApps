using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.MediaMuxer.Models;

namespace HanumanInstitute.MediaMuxer
{
    /// <summary>
    /// Contains selection data to pass between wizard pages.
    /// </summary>
    public class MuxerService
    {
        private IMediaInfoReader _mediaInfo;
        private IMediaMuxer _mediaMuxer;

        public MuxerService(IMediaInfoReader mediaInfo, IMediaMuxer mediaMuxer)
        {
            _mediaInfo = mediaInfo;
            _mediaMuxer = mediaMuxer;
        }

        public ObservableCollection<FileItem> Files = new ObservableCollection<FileItem>();
        public ObservableCollection<MediaStream> FileStreams = new ObservableCollection<MediaStream>();
        public string OutputFile;

        public void Clear()
        {
            Files.Clear();
            FileStreams.Clear();
            OutputFile = null;
        }

        public List<MediaStreamInfo> GetStreamList(string file)
        {
            return _mediaInfo.GetFileInfo(file, new ProcessOptionsEncoder(ProcessDisplayMode.None)).FileStreams;
        }

        public bool StartMuxe()
        {
            return _mediaMuxer.Muxe(FileStreams, OutputFile, new ProcessOptionsEncoder(ProcessDisplayMode.Interface, "Muxing Media Files")) == CompletionStatus.Success;
        }

        public bool StartMerge()
        {
            return _mediaMuxer.Concatenate(Files.Select(f => f.Path).ToList(), OutputFile, new ProcessOptionsEncoder(ProcessDisplayMode.Interface, "Merging Media Files")) == CompletionStatus.Success;
        }

        public bool StartSplit()
        {
            return false;
            // return FfmpegBusiness.JoinAudioVideoMuxer(FileStreams.Cast<FfmpegStream>(), OutputFile, false, true);
        }
    }
}
