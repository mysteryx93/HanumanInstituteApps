using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.AudioVideoMuxer {
    /// <summary>
    /// Contains selection data to pass between wizard pages.
    /// </summary>
    public class MuxerBusiness {
        public ObservableCollection<FileItem> Files = new ObservableCollection<FileItem>();
        public ObservableCollection<FFmpegStream> FileStreams = new ObservableCollection<FFmpegStream>();
        public string OutputFile;

        public void Clear() {
            Files.Clear();
            FileStreams.Clear();
            OutputFile = null;
        }

        public List<FFmpegStreamInfo> GetStreamList(string file) {
            return MediaInfo.GetFileInfo(file, new ProcessStartOptions(FFmpegDisplayMode.None)).FileStreams;
        }

        public bool StartMuxe() {
            return MediaMuxer.Muxe(FileStreams, OutputFile, new ProcessStartOptions(FFmpegDisplayMode.Interface, "Muxing Media Files")) == CompletionStatus.Success;
        }

        public bool StartMerge() {
            return MediaMuxer.ConcatenateFiles(Files.Select(f => f.Path).ToList(), OutputFile, new ProcessStartOptions(FFmpegDisplayMode.Interface, "Merging Media Files")) == CompletionStatus.Success;
        }

        public bool StartSplit() {
            return false;
            // return FfmpegBusiness.JoinAudioVideoMuxer(FileStreams.Cast<FfmpegStream>(), OutputFile, false, true);
        }
    }
}
