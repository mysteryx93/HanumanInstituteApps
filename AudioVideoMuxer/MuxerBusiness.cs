using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;

namespace AudioVideoMuxer {
    /// <summary>
    /// Contains selection data to pass between wizard pages.
    /// </summary>
    public class MuxerBusiness {
        public ObservableCollection<FileItem> Files = new ObservableCollection<FileItem>();
        public ObservableCollection<FfmpegStream> FileStreams = new ObservableCollection<FfmpegStream>();
        public string OutputFile;

        public void Clear() {
            Files.Clear();
            FileStreams.Clear();
            OutputFile = null;
        }

        public List<FfmpegStream> GetStreamList(string file) {
            return FfmpegBusiness.GetStreamList(file);
        }

        public bool StartMuxe() {
            return FfmpegBusiness.JoinAudioVideoMuxer(FileStreams, OutputFile, false, true);
        }

        public bool StartMerge() {
            return FfmpegBusiness.ConcatenateFiles(Files.Select(f => f.Path).ToList(), OutputFile, true);
        }

        public bool StartSplit() {
            return false;
            // return FfmpegBusiness.JoinAudioVideoMuxer(FileStreams.Cast<FfmpegStream>(), OutputFile, false, true);
        }
    }
}
