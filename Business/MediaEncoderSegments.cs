using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmergenceGuardian.FFmpeg;
using System.Threading.Tasks;

namespace Business {
    /// <summary>
    /// Manages video segments to determine what is done and what needs to be done.
    /// </summary>
    public class MediaEncoderSegments {
        private MediaEncoderSettings settings;
        private const int SegSize = 1000;

        /// <summary>
        /// Returns the total amount of frames in the script.
        /// </summary>
        public long TotalFrames { get; private set; }
        /// <summary>
        /// Returns the list of segments that have been encoded.
        /// </summary>
        public List<SegmentInfo> SegDone { get; private set; }
        /// <summary>
        /// Returns the list of segments that still require processing.
        /// </summary>
        public List<SegmentInfo> SegLeft { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MediaEncoderSegments class.
        /// </summary>
        public MediaEncoderSegments() {
        }

        /// <summary>
        /// Analyzes output files to determine what work is done and what needs to be done.
        /// </summary>
        /// <param name="settings">The media encoder settings of the job to scan.</param>
        /// <returns>Whether job needs to execute.</returns>
        public void Analyze(MediaEncoderSettings settings) {
            int Threads = settings.ParallelProcessing;
            if (Threads == 0)
                Threads = 1;
            this.settings = settings;
            SegDone = new List<SegmentInfo>();
            SegLeft = new List<SegmentInfo>();

            // Get script total frame count, and run in background until all other files are scanned.
            AvisynthTools.EditStartPosition(settings.ScriptFile, 0, 0);
            ProcessStartOptions Options = new ProcessStartOptions(settings.JobIndex, "Analyzing Segments...", false).TrackProcess(settings);
            Task<long> TaskCount = Task.Run(() => AvisynthTools.GetFrameCount(settings.ScriptFile, Options));

            // Get list of output files in folder. The number after "_Output_" is the frame position of that segment.
            string FileName = string.Format("Job{0}_Output_", settings.JobIndex);
            string FileNameExt = string.Format(".{0}", settings.Container);
            string[] SegFiles = Directory.GetFiles(Settings.TempFilesPath, FileName + "*" + FileNameExt);

            // Scan each segment file and discard files smaller than 10kb or of less than 10 frames.
            // Create a list of tasks to run them all in parallel.
            List<Task<KeyValuePair<string, long>>> TaskList = new List<Task<KeyValuePair<string, long>>>();
            foreach (string seg in SegFiles) {
                if (settings.CompletionStatus == CompletionStatus.Cancelled)
                    break;
                // Discard empty files.
                if (new FileInfo(seg).Length > 0) {
                    // string SegmentLocal = seg;
                    TaskList.Add(Task.Run(() => {
                        return new KeyValuePair<string, long>(seg, MediaInfo.GetFrameCount(seg, null));
                    }));
                } else
                    File.Delete(seg);
            }

            // Run all segment length queries simultaneously and analyze results to fill SegDone.
            Task.WaitAll(TaskList.ToArray());
            string SegFile;
            long SegStart;
            bool SegAdded;
            int Pos;
            string SegText;
            if (settings.CompletionStatus != CompletionStatus.Cancelled) {
                foreach (var item in TaskList) {
                    SegFile = item.Result.Key;
                    SegStart = 0;
                    SegAdded = false;
                    if (item.Result.Value >= 1) { // Segment must contain at least one valid frame.
                        Pos = SegFile.IndexOf(FileName);
                        if (Pos > -1) {
                            Pos += FileName.Length;
                            SegText = SegFile.Substring(Pos, SegFile.Length - Pos - FileNameExt.Length);
                            if (long.TryParse(SegText, out SegStart)) {
                                SegDone.Add(new SegmentInfo(SegStart, SegStart + item.Result.Value));
                                SegAdded = true;
                            }
                        }
                    }
                    if (!SegAdded)
                        File.Delete(SegFile);
                }
                // Order can be random, must sort.
                SegDone = SegDone.OrderBy(s => s.Start).ToList();
            }

            // Get script total frames and calculate the work left.
            TaskCount.Wait();
            if (settings.CompletionStatus == CompletionStatus.Cancelled) {
                SegDone.Clear();
            } else {
                // Create list of segments left.
                TotalFrames = TaskCount.Result;
                long SegPos = 0;
                foreach (SegmentInfo segd in SegDone) {
                    if (segd.Start > SegPos)
                        SegLeft.Add(new SegmentInfo(SegPos, segd.Start - 1));
                    SegPos = segd.End + 1;
                }
                if (SegPos < TotalFrames)
                    SegLeft.Add(new SegmentInfo(SegPos, TotalFrames - 1));

                if (settings.ParallelProcessing > 1) {
                    // Divide in segments
                    int Instances = settings.ParallelProcessing;
                    int SmallSegSize = SegSize / Instances;
                    int StartSegSize = ((Instances - 1) * Instances / 2) * SmallSegSize;
                    SegmentInfo Seg;
                    int SegCount = 0;
                    long IdealSeg, AvailSeg;

                    // Work on copy because original list will be modified.
                    List<SegmentInfo> SegLeftCopy = SegLeft.ToList();
                    SegLeft.Clear();

                    for (int i = 0; i < SegLeftCopy.Count(); i++) {
                        Seg = SegLeftCopy[i];
                        AvailSeg = Seg.Length;
                        while (AvailSeg > 0) {
                            // Start with smaller segments.
                            IdealSeg = SegCount < Instances ? ++SegCount * SmallSegSize : SegSize;
                            // Split segment only if it is larger than threshold
                            if (AvailSeg > IdealSeg + SmallSegSize) {
                                SegLeft.Add(new SegmentInfo(Seg.Start, Seg.Start + IdealSeg - 1));
                                Seg.Start += IdealSeg;
                                AvailSeg -= IdealSeg;
                            } else {
                                SegLeft.Add(new SegmentInfo(Seg.Start, Seg.End));
                                AvailSeg = 0;
                            }
                        }
                    }

                    // If we're still missing segments (for short clips), split using other method.
                    // Create segments to reach the desired amount of threads.
                    int SegMissing = SegLeft.Count > 0 ? Threads - SegLeft.Count() : 0;
                    for (int i = 0; i < SegMissing; i++) {
                        // Find largest segment.
                        int SegMaxIndex = 0;
                        long SegMax = SegLeft[0].Length;
                        for (int j = 1; j < SegLeft.Count(); j++) {
                            if (SegLeft[j].Length > SegMax) {
                                SegMaxIndex = j;
                                SegMax = SegLeft[j].Length;
                            }
                        }
                        // Split largest segment in half.
                        Seg = SegLeft[SegMaxIndex];
                        if (Seg.Length > 80) { // Only split if segment has at least 80 frames (creating segments of 40).
                            long SegSep = Seg.Start + (Seg.Length - 1) / 2;
                            SegLeft[SegMaxIndex] = new SegmentInfo(Seg.Start, SegSep);
                            SegLeft.Insert(SegMaxIndex + 1, new SegmentInfo(SegSep + 1, Seg.End));
                        }
                    }
                }
            }
        }
    }

    public class SegmentInfo {
        public SegmentInfo() { }

        public SegmentInfo(long start, long end) {
            Start = start;
            End = end;
        }

        public long Start { get; set; }
        public long End { get; set; }

        public long Length {
            get {
                return End - Start + 1;
            }
        }
    }
}
