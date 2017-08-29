using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;
using DataAccess;
using EmergenceGuardian.FFmpeg;
using System.Threading;
using System.Windows;

namespace Business {
    public class MediaEncoderBusiness {
        private int JobIndex = 0;
        public ObservableCollection<MediaEncoderSettings> ProcessingQueue = new ObservableCollection<MediaEncoderSettings>();
        public bool IsEncoding { get; private set; } = false;
        private Thread JobThread = null;
        public MediaEncoderSettings DeshakerSourceSettings { get; set; }

        public event EventHandler<EncodingCompletedEventArgs> EncodingCompleted;
        public event EventHandler<EncodingCompletedEventArgs> EncodingFailed;

        public void GenerateScript(MediaEncoderSettings settings, bool preview, bool multiThreaded) {
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
            if (Script.IsEmpty)
                Script = MediaEncoderScript.GenerateVideoScript(settings, GetPreviewSourceFile(settings), preview, multiThreaded);
            else if (preview) {
                Script.RemoveMT();
                Script.AppendLine(@"ConvertToRGB32(matrix=""Rec709"")");
            } else if (!multiThreaded)
                Script.RemoveMT();
            Script.WriteToFile(PathManager.PreviewScriptFile);
            settings.Save(PathManager.PreviewSettingsFile);
        }

        public void GenerateCustomScript(MediaEncoderSettings settings) {
            settings.CustomScript = MediaEncoderScript.GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true).Script;
        }

        public bool CustomScriptHasChanges(MediaEncoderSettings settings) {
            return settings.CustomScript.Replace("\r\n", "\n") != MediaEncoderScript.GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true).Script.Replace("\r\n", "\n");
        }

        public string GetPreviewSourceFile(MediaEncoderSettings settings) {
            if (settings.ConvertToAvi)
                return PathManager.PreviewSourceFile;
            else
                return settings.FilePath;
        }

        public async Task DeletePreviewFilesAsync() {
            for (int i = 0; i < 5; i++) {
                try {
                    File.Delete(PathManager.PreviewSourceFile);
                    File.Delete(PathManager.PreviewScriptFile);
                    File.Delete(PathManager.PreviewSettingsFile);
                    File.Delete(PathManager.PreviewDeshakerScript);
                    File.Delete(PathManager.PreviewDeshakerLog);
                    return;
                }
                catch {
                }
                await Task.Delay(200);
            }
        }

        public void AddJobToQueue(MediaEncoderSettings settings) {
            ProcessingQueue.Add(settings);
            if (IsEncoding && ProcessingQueue.Count == 1)
                StartEncoderThread();
        }

        public void ResumeEncoding() {
            if (IsEncoding)
                return;

            IsEncoding = true;
            StartEncoderThread();
        }

        public bool PauseEncoding() {
            if (IsEncoding) {
                IsEncoding = false;
                MediaEncoderSettings Job = ProcessingQueue.FirstOrDefault();
                if (Job != null) {
                    Job.CompletionStatus = CompletionStatus.Cancelled;
                    FFmpegConfig.UserInterfaceManager.Stop(Job.JobIndex);
                    return true;
                }
            }
            return false;
        }

        private void StartEncoderThread() {
            if (JobThread == null && ProcessingQueue.Count > 0) {
                JobThread = new Thread(EncoderThread);
                JobThread.Start(ProcessingQueue.First());
            }
        }

        private void EncoderThread(object obj) {
            MediaEncoderSettings settings = obj as MediaEncoderSettings;
            settings.CompletionStatus = CompletionStatus.Success;
            DateTime StartTime = DateTime.Now;

            FFmpegConfig.UserInterfaceManager.Start(settings.JobIndex, "Processing Video");

            MediaEncoderSegments SegBusiness = PrepareExistingJob(settings);
            // If merging is completed, SegBusiness==null. If work is completed but not merged, SegBusiness.SegLeft = empty list.
            if (SegBusiness != null && SegBusiness.SegLeft.Count() > 0) {
                if (settings.Deshaker && (settings.DeshakerSettings.PrescanAction != PrescanType.Full || !settings.DeshakerSettings.PrescanCompleted)) {
                    settings.DeshakerSettings.PrescanAction = PrescanType.Full;
                    settings.CompletionStatus = GenerateDeshakerLog(settings, settings.InputFile);
                }

                if (settings.CompletionStatus == CompletionStatus.Success) {
                    // Encode audio stream
                    Task EncAudio = null;
                    if (settings.HasAudioOptions)
                        EncAudio = Task.Run(() => AvisynthTools.EncodeAudio(settings));

                    // Encode video stream in segments
                    List<Task<CompletionStatus>> EncTasks = new List<Task<CompletionStatus>>();
                    if (settings.VideoAction != VideoAction.Copy) {
                        bool Cancel = false;
                        foreach (SegmentInfo seg in SegBusiness.SegLeft) {
                            MediaEncoderSettings EncSettings = settings.Clone();
                            EncSettings.ResumePos = seg.Start;
                            File.Delete(EncSettings.OutputFile);
                            EncTasks.Add(Task.Run(() => AvisynthTools.EncodeVideo(EncSettings, seg.Length, SegBusiness.TotalFrames)));

                            // If there are more segments than max parallel instances, wait until some threads finish
                            if (EncTasks.Count >= settings.ParallelProcessing) {
                                Task.WaitAny(EncTasks.ToArray());
                                foreach (var item in EncTasks.ToArray()) {
                                    if (item.IsCompleted) {
                                        if (item.Result == CompletionStatus.Success)
                                            EncTasks.Remove(item);
                                        else {
                                            settings.CompletionStatus = item.Result;
                                            Cancel = true;
                                        }
                                    }
                                }
                            }
                            if (Cancel)
                                break;
                        }
                    }

                    EncAudio?.Wait();
                    Task.WaitAll(EncTasks.ToArray());
                }
            } else if (settings.CompletionStatus == CompletionStatus.None)
                settings.CompletionStatus = CompletionStatus.Success;

            if (FFmpegConfig.UserInterfaceManager.AppExited)
                return;

            // Check if encode is completed.
            EncodingCompletedEventArgs CompletedArgs = null;
            if (settings.CompletionStatus == CompletionStatus.Success) {
                CompletedArgs = FinalizeEncoding(settings, StartTime);
                if (settings.CompletionStatus == CompletionStatus.Success && CompletedArgs != null)
                    EncodingCompleted?.Invoke(this, CompletedArgs);

            }
            if (settings.CompletionStatus != CompletionStatus.Success) {
                CompletedArgs = GetEncodingResults(settings, null, StartTime);
                if (IsEncoding)
                    EncodingFailed?.Invoke(this, CompletedArgs);
            }

            if (IsEncoding || settings.CompletionStatus == CompletionStatus.Success)
                Application.Current.Dispatcher.Invoke(() => ProcessingQueue.Remove(settings));

            FFmpegConfig.UserInterfaceManager.Stop(settings.JobIndex);

            // Start next job.
            if (IsEncoding && ProcessingQueue.Count > 0)
                EncoderThread(ProcessingQueue.First());

            JobThread = null;
        }

        private EncodingCompletedEventArgs FinalizeEncoding(MediaEncoderSettings settings, DateTime? startTime) {
            settings.ResumePos = -1;
            string VideoFile = settings.VideoAction == VideoAction.Discard ? null : settings.VideoAction == VideoAction.Copy ? settings.FilePath : settings.OutputFile;

            if (!File.Exists(settings.OutputFile) && settings.VideoAction != VideoAction.Copy) {
                // Merge segments.
                settings.CompletionStatus = MergeSegments(settings, VideoFile);
                if (settings.CompletionStatus != CompletionStatus.Success)
                    return null;
            }

            if (!File.Exists(settings.FinalFile)) {
                // Muxe video with audio.
                string AudioFile = settings.AudioAction == AudioActions.Discard ? null : settings.AudioAction == AudioActions.Copy ? settings.FilePath : settings.AudioFile;
                if (AudioFile == null)
                    File.Move(VideoFile, settings.FinalFile);
                else if (VideoFile == null)
                    File.Move(AudioFile, settings.FinalFile);
                else
                    settings.CompletionStatus = MediaMuxer.Muxe(VideoFile, AudioFile, settings.FinalFile, new ProcessStartOptions(settings.JobIndex, "Muxing Audio and Video", false));
                if (settings.CompletionStatus != CompletionStatus.Success)
                    return null;

            }

            return GetEncodingResults(settings, settings.FinalFile, startTime);
        }

        /// <summary>
        /// For files encoded in various segments (stop/resume), merge the various segments.
        /// </summary>
        private CompletionStatus MergeSegments(MediaEncoderSettings settings, string destination) {
            MediaEncoderSegments segBusiness = new MediaEncoderSegments();
            segBusiness.Analyze(settings);
            if (segBusiness.SegLeft.Count() > 0)
                return CompletionStatus.Error;

            List<string> SegmentList = new List<string>();
            foreach (SegmentInfo seg in segBusiness.SegDone) {
                SegmentList.Add(PathManager.GetOutputFile(settings.JobIndex, seg.Start, settings.Container));
            }

            CompletionStatus Result = CompletionStatus.Success;
            File.Delete(destination);
            if (SegmentList.Count == 1)
                File.Move(SegmentList[0], destination);
            else if (SegmentList.Count > 1) {
                Result = MediaMuxer.Concatenate(SegmentList, destination, new ProcessStartOptions(settings.JobIndex, "Merging Files", false));
            }
            settings.CompletionStatus = Result;
            return Result;
        }

        private EncodingCompletedEventArgs GetEncodingResults(MediaEncoderSettings settings, string finalFile, DateTime? startTime) {
            // Create encoding result object.
            EncodingCompletedEventArgs Result = null;
            if (finalFile == null || File.Exists(finalFile)) {
                Result = new EncodingCompletedEventArgs();
                Result.Settings = settings;
                Result.OldFileName = settings.FilePath;
                Result.NewFileName = settings.FinalFile;
                if (startTime.HasValue)
                    Result.EncodingTime = DateTime.Now - startTime.Value;
                if (finalFile != null) {
                    FileInfo FinalFileInfo = new FileInfo(finalFile);
                    Result.NewFileSize = FinalFileInfo.Length;
                    FinalFileInfo = new FileInfo(settings.FilePath);
                    Result.OldFileSize = FinalFileInfo.Length;
                }
            }
            return Result;
        }

        public async Task PreparePreviewFile(MediaEncoderSettings settings, bool overwrite, bool calcAutoCrop) {
            if (string.IsNullOrEmpty(settings.FilePath))
                return;

            if (overwrite) {
                File.Delete(PathManager.PreviewSourceFile);
                // Select default open method.
                if (settings.FilePath.ToLower().EndsWith(".avi"))
                    settings.ConvertToAvi = false;
                else {
                    FFmpegProcess FileInfo = await Task.Run(() => MediaInfo.GetFileInfo(settings.FilePath));
                    if (settings.ConvertToAvi && FileInfo?.VideoStream?.Height >= 720)
                        settings.ConvertToAvi = false;
                }
            }

            bool AviFileReady = File.Exists(PathManager.PreviewSourceFile);
            if (!AviFileReady && settings.ConvertToAvi)
                AviFileReady = await Task.Run(() => MediaEncoder.ConvertToAvi(settings.FilePath, PathManager.PreviewSourceFile, true, new ProcessStartOptions(FFmpegDisplayMode.Interface, "Converting to AVI"))) == CompletionStatus.Success;

            if (AviFileReady && settings.ConvertToAvi)
                await GetMediaInfo(PathManager.PreviewSourceFile, settings);
            else {
                settings.ConvertToAvi = false;
                await GetMediaInfo(settings.FilePath, settings);
            }

            // Auto-calculate crop settings.
            if (calcAutoCrop) {
                if (settings.CropLeft == 0 && settings.CropTop == 0 && settings.CropRight == 0 && settings.CropBottom == 0) {
                    Rect AutoCrop = await Task.Run(() => AvisynthTools.GetAutoCropRect(settings, null));
                    if (settings.CropLeft == 0)
                        settings.CropLeft = AutoCrop.Left;
                    if (settings.CropTop == 0)
                        settings.CropTop = AutoCrop.Top;
                    if (settings.CropRight == 0)
                        settings.CropRight = AutoCrop.Right;
                    if (settings.CropBottom == 0)
                        settings.CropBottom = AutoCrop.Bottom;
                }
            }
        }

        public void PrepareJobFiles(MediaEncoderSettings settings) {
            settings.JobIndex = ++JobIndex;
            // Files must be prepared before adding to queue so that user can replace preview files.
            PathManager.DeleteJobFiles(settings.JobIndex);
            File.Delete(PathManager.PreviewScriptFile);
            File.Delete(PathManager.PreviewSettingsFile);
            if (settings.ConvertToAvi)
                PathManager.SafeMove(PathManager.PreviewSourceFile, settings.InputFile);
            if (settings.Deshaker)
                PathManager.SafeMove(PathManager.PreviewDeshakerLog, settings.DeshakerLog);
            settings.Save(settings.SettingsFile);
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
            if (Script.IsEmpty)
                Script = MediaEncoderScript.GenerateVideoScript(settings, settings.InputFile, false, true);
            else {
                Script.Replace(Script.GetAsciiPath(PathManager.PreviewSourceFile), Script.GetAsciiPath(settings.InputFile));
                Script.Replace(Script.GetAsciiPath(PathManager.PreviewDeshakerLog), Script.GetAsciiPath(settings.DeshakerLog));
            }
            Script.WriteToFile(settings.ScriptFile);
            // if (settings.DeshakerSettings.PrescanAction == PrescanType.Full)
        }

        /// <summary>
        /// Prepares the files of an existing job.
        /// </summary>
        /// <returns>A MediaEncoderSegments object containing the segments analysis.</returns>
        public MediaEncoderSegments PrepareExistingJob(MediaEncoderSettings settings) {
            settings.ResumePos = 0;
            settings.CompletionStatus = CompletionStatus.Success;
            if (File.Exists(settings.FinalFile)) {
                // Merging was completed.
                return null;
            } else {
                MediaEncoderSegments SegBusiness = new MediaEncoderSegments();
                SegBusiness.Analyze(settings);
                return SegBusiness;
            }
        }


        /// <summary>
        /// Prepares the files of an existing job that we resume.
        /// </summary>
        /// <returns>Whether job still needs to execute.</returns>
        //private bool PrepareResumeJob(MediaEncoderSettings settings) {
        //    AvisynthTools.EditStartPosition(settings.ScriptFile, 0);
        //    // At least one segment has been processed. Check if the entire file has been processed.
        //    ProcessStartOptions Options = new ProcessStartOptions(settings.JobIndex, "Resuming...", false).TrackProcess(settings);
        //    Task<long> TaskCount = Task.Run(() => AvisynthTools.GetFrameCount(settings.ScriptFile, Options));

        //    int Segment = 0;
        //    List<Task<long>> TaskList = new List<Task<long>>();
        //    while (File.Exists(PathManager.GetOutputFile(settings.JobIndex, ++Segment, settings.VideoCodec)) && settings.CompletionStatus != CompletionStatus.Cancelled) {
        //        string SegmentFile = PathManager.GetOutputFile(settings.JobIndex, Segment, settings.VideoCodec);
        //        // Discard segments of less than 10kb.
        //        if (new FileInfo(SegmentFile).Length > 10000) {
        //            int SegmentLocal = Segment;
        //            TaskList.Add(Task.Run(() => AvisynthTools.GetFrameCount(PathManager.GetOutputFile(settings.JobIndex, SegmentLocal, settings.VideoCodec), null)));
        //        } else {
        //            // There shouldn't be any resumed job following a segment that is being deleted.
        //            File.Delete(SegmentFile);
        //            break;
        //        }
        //    }

        //    long OutputFrames = 0;
        //    Task.WaitAll(TaskList.ToArray());
        //    foreach (Task<long> item in TaskList) {
        //        OutputFrames += item.Result;
        //    }

        //    TaskCount.Wait();
        //    if (settings.CompletionStatus == CompletionStatus.Cancelled)
        //        return false;

        //    long ScriptFrames = TaskCount.Result;
        //    if (OutputFrames >= ScriptFrames) {
        //        // Job completed.
        //        //EncodingCompletedEventArgs EncodeResult = FinalizeEncoding(settings, null);
        //        //if (EncodeResult != null)
        //        //    EncodingCompleted(this, EncodeResult);
        //        //else {
        //        //    PathManager.DeleteJobFiles(settings.JobIndex);
        //        //}
        //        return false;
        //    } else {
        //        // Resume with new segment.
        //        AvisynthTools.EditStartPosition(settings.ScriptFile, OutputFrames);
        //        settings.ResumeSegment = Segment;
        //        settings.ResumePos = OutputFrames;
        //        File.Delete(settings.OutputFile);
        //        return true;
        //    }
        //}

        private async Task GetMediaInfo(string previewFile, MediaEncoderSettings settings) {
            FFmpegProcess FInfo = await Task.Run(() => MediaInfo.GetFileInfo(previewFile));
            FFmpegVideoStreamInfo VInfo = FInfo.VideoStream;
            FFmpegAudioStreamInfo AInfo = settings.ConvertToAvi ? await Task.Run(() => MediaInfo.GetFileInfo(settings.FilePath).AudioStream) : FInfo.AudioStream;

            settings.SourceWidth = FInfo.VideoStream.Width;
            settings.SourceHeight = FInfo.VideoStream.Height;
            if (settings.SourceHeight > 768)
                settings.OutputHeight = settings.SourceHeight.Value;
            settings.SourceAspectRatio = (float)VInfo.PixelAspectRatio;
            // Fix last track of VCDs that is widescreen.
            if (settings.SourceHeight == 288 && settings.SourceWidth == 352 && settings.SourceAspectRatio == 1.485f)
                settings.SourceAspectRatio = 1.092f;
            settings.SourceFrameRate = VInfo.FrameRate;

            settings.SourceAudioFormat = AInfo.Format;
            settings.SourceVideoFormat = VInfo.Format;
            bool IsTvRange = VInfo.ColorRange != "pc";
            if (!string.IsNullOrEmpty(VInfo.ColorMatrix)) {
                settings.SourceColorMatrix = VInfo.ColorMatrix.EndsWith("601") ? (IsTvRange ? ColorMatrix.Rec601 : ColorMatrix.Pc601) : (IsTvRange ? ColorMatrix.Rec709 : ColorMatrix.Pc709);
            } else
                settings.SourceColorMatrix = VInfo.Height < 600 ? (IsTvRange ? ColorMatrix.Rec601 : ColorMatrix.Pc601) : (IsTvRange ? ColorMatrix.Rec709 : ColorMatrix.Pc709);
            settings.SourceChromaPlacement = string.Compare(VInfo.Format, "mpeg1video", true) == 0 ? ChromaPlacement.MPEG1 : ChromaPlacement.MPEG2;
            settings.DegrainPrefilter = VInfo.Height < 600 ? DegrainPrefilters.SD : DegrainPrefilters.HD;

            settings.SourceVideoBitrate = (int)(new FileInfo(previewFile).Length / FInfo.FileDuration.TotalSeconds / 1024 * 8);
            settings.SourceAudioBitrate = AInfo.Bitrate;
            if (!settings.HasAudioOptions)
                settings.AudioQuality = AInfo.Bitrate > 0 ? AInfo.Bitrate : 256;
            if (settings.AudioQuality > 384)
                settings.AudioQuality = 384;
            settings.SourceBitDepth = 8; // VInfo.BitDepth;
            settings.DenoiseD = 2;
            settings.DenoiseA = settings.SourceHeight < 720 ? 2 : 1;
            settings.Position = FInfo.FileDuration.TotalSeconds / 2;
            settings.VideoAction = settings.SourceHeight >= 1080 ? VideoAction.x264 : VideoAction.x265;
            settings.EncodeQuality = settings.SourceHeight >= 1080 ? 23 : 22;
            settings.EncodePreset = settings.SourceHeight >= 1080 ? EncodePresets.veryslow : EncodePresets.medium;
            // Use Cache to open file when file is over 500MB
            settings.CalculateSize();
        }

        public void FinalizeReplace(EncodingCompletedEventArgs jobInfo) {
            EditVideoBusiness EditBusiness = new EditVideoBusiness();
            string RelativePath = jobInfo.OldFileName.StartsWith(Settings.NaturalGroundingFolder) ? jobInfo.OldFileName.Substring(Settings.NaturalGroundingFolder.Length) : jobInfo.OldFileName;
            Media EditVideo = EditBusiness.GetVideoByFileName(RelativePath);
            System.Threading.Thread.Sleep(200); // Give MPC time to release the file.
            string OriginalPath = Path.Combine(Path.GetDirectoryName(jobInfo.OldFileName), "Original", Path.GetFileName(jobInfo.OldFileName));
            string NewPath = PathManager.GetPathWithoutExtension(jobInfo.OldFileName) + Path.GetExtension(jobInfo.Settings.FinalFile);
            Directory.CreateDirectory(Path.GetDirectoryName(OriginalPath));
            PathManager.SafeMove(jobInfo.OldFileName, OriginalPath);
            PathManager.SafeMove(jobInfo.Settings.FinalFile, NewPath);
            jobInfo.Settings.FilePath = OriginalPath.Substring(Settings.NaturalGroundingFolder.Length);

            if (EditVideo != null) {
                EditVideo.FileName = NewPath.Substring(Settings.NaturalGroundingFolder.Length);
                EditVideo.Height = null;
                EditBusiness.Save();
            }
        }

        public void FinalizeKeep(EncodingCompletedEventArgs jobInfo) {
            string FinalFile = String.Format("{0} - Encoded.{1}", PathManager.GetPathWithoutExtension(jobInfo.OldFileName), jobInfo.Settings.Container);
            PathManager.SafeMove(jobInfo.NewFileName, FinalFile);
        }

        /// <summary>
        /// Moves specified settings file as preview files.
        /// </summary>
        /// <param name="settings">The settings to use for re-encoding.</param>
        public async Task MovePreviewFilesAsync(MediaEncoderSettings settings) {
            await DeletePreviewFilesAsync();
            if (settings.ConvertToAvi)
                File.Move(settings.InputFile, PathManager.PreviewSourceFile);
            if (settings.Deshaker && File.Exists(settings.DeshakerLog))
                File.Move(settings.DeshakerLog, PathManager.PreviewDeshakerLog);
            if (!string.IsNullOrEmpty(settings.CustomScript) && (settings.ConvertToAvi || settings.Deshaker)) {
                AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
                if (settings.ConvertToAvi)
                    Script.Replace(Script.GetAsciiPath(settings.InputFile), Script.GetAsciiPath(PathManager.PreviewSourceFile));
                if (settings.Deshaker)
                    Script.Replace(Script.GetAsciiPath(settings.DeshakerLog), Script.GetAsciiPath(PathManager.PreviewDeshakerLog));
                settings.CustomScript = Script.Script;
            }
            settings.Save(PathManager.PreviewSettingsFile);
        }

        /// <summary>
        /// Auto-load Preview file if encoded was unexpectedly closed.
        /// </summary>
        /// <returns>The previous preview encoding settings.</returns>
        public async Task<MediaEncoderSettings> AutoLoadPreviewFileAsync() {
            if (File.Exists(PathManager.PreviewSettingsFile)) {
                MediaEncoderSettings settings = MediaEncoderSettings.Load(PathManager.PreviewSettingsFile);
                if (!File.Exists(PathManager.PreviewSourceFile) && File.Exists(settings.FilePath)) {
                    double? SourceFps = settings.SourceFrameRate; // Keep FPS in case it cannot be read from the file.
                    await PreparePreviewFile(settings, false, false);
                    if (!settings.SourceFrameRate.HasValue)
                        settings.SourceFrameRate = SourceFps;
                }
                settings.JobIndex = -1;
                return settings;
            }
            return null;
        }

        /// <summary>
        /// Automatically reloads jobs if the encoder was unexpectedly closed.
        /// </summary>
        public void AutoLoadJobs() {
            try {
                if (!Directory.Exists(Settings.TempFilesPath))
                    Directory.CreateDirectory(Settings.TempFilesPath);
            }
            catch {
                return;
            }

            var JobList = Directory.EnumerateFiles(Settings.TempFilesPath, "Job*_Settings.xml");
            int Index = 0;
            MediaEncoderSettings settings;
            //List<Task> TaskList = new List<Task>();

            // Get list of interrupted jobs.
            foreach (string item in JobList) {
                // Load settings file.
                settings = null;
                try {
                    settings = MediaEncoderSettings.Load(item);
                }
                catch { }

                // Resume job.
                if (int.TryParse(Path.GetFileName(item).Replace("Job", "").Replace("_Settings.xml", ""), out Index)) {
                    if (Index > JobIndex)
                        JobIndex = Index;

                    if (settings != null && File.Exists(settings.InputFile) && File.Exists(settings.ScriptFile) && File.Exists(settings.SettingsFile) && File.Exists(settings.FilePath)) {
                        ProcessingQueue.Add(settings);
                        //TaskList.Add(StartJobAsync(settings));
                    } else {
                        // Resume job failed, delete files.
                        PathManager.DeleteJobFiles(settings.JobIndex);
                    }
                }
            }
        }

        public CompletionStatus GenerateDeshakerLog(MediaEncoderSettings settings, string inputFile) {
            // Prepare Deshaker settings.
            settings.DeshakerSettings.Pass = 1;
            settings.DeshakerSettings.LogFile = settings.DeshakerTempLog;
            settings.DeshakerSettings.SourcePixelAspectRatio = settings.SourceAspectRatio.Value;
            // settings.DeshakerSettings.AppendToFile = true;
            File.Delete(settings.DeshakerLog);

            // Start UI.
            CompletionStatus Result = CompletionStatus.Success;
            object JobId = "Deshaker";
            FFmpegConfig.UserInterfaceManager.Start(JobId, "Running Deshaker Prescan");
            ProcessStartOptions JobOptions = new ProcessStartOptions(JobId, "Getting Frame Count", false);

            // Get frame count.
            settings.CalculateSize();
            int FrameStart = (int)((settings.DeshakerSettings.PrescanStart ?? 0) * settings.SourceFrameRate.Value);
            int FrameEnd = (int)((settings.DeshakerSettings.PrescanEnd ?? 0) * settings.SourceFrameRate.Value);
            string Script = MediaEncoderScript.GenerateDeshakerScript(settings, inputFile, 0, FrameStart, FrameEnd);
            File.WriteAllText(settings.DeshakerScript, Script);
            JobOptions.FrameCount = AvisynthTools.GetFrameCount(settings.DeshakerScript, JobOptions);

            // Pad file start.
            using (StreamWriter sw = new StreamWriter(File.Open(settings.DeshakerLog, FileMode.Create), Encoding.ASCII)) {
                for (int i = 0; i < FrameStart; i++) {
                    sw.WriteLine((i + 1).ToString().PadLeft(7) + "									skipped		#	   0.00	   0.00	");
                }
            }

            // Run segments.
            var Segments = settings.DeshakerSettings.Segments;
            if (JobOptions.FrameCount > 0) {
                for (int i = 0; i < Segments.Count; i++) {
                    // Get start position of next segment.
                    long NextSegmentStart = i < Segments.Count - 1 ? Segments[i + 1].FrameStart : 0;
                    if (NextSegmentStart == 0 || NextSegmentStart > FrameStart) { // Enforce PrescanStart for preview
                        long SegmentStart = Segments[i].FrameStart;
                        long SegmentEnd = NextSegmentStart > 0 ? NextSegmentStart - 1 : 0;
                        // Enforce PrescanEnd for preview
                        if ((FrameEnd > 0 && SegmentStart > FrameEnd) || (SegmentEnd > 0 && SegmentStart > SegmentEnd))
                            break;
                        if ((FrameStart > 0 && FrameStart > SegmentStart) || SegmentStart == 0)
                            SegmentStart = FrameStart;
                        if ((FrameEnd > 0 && FrameEnd < SegmentEnd) || SegmentEnd == 0)
                            SegmentEnd = FrameEnd;
                        Result = GenerateDeshakerLogSegment(settings, inputFile, i, FrameStart, SegmentStart, SegmentEnd, JobOptions);
                        if (Result != CompletionStatus.Success)
                            break;

                        // Merge log segment into log file and set right frame numbers.
                        using (StreamWriter sw = new StreamWriter(File.Open(settings.DeshakerLog, FileMode.Append), Encoding.ASCII)) {
                            using (StreamReader sr = new StreamReader(File.OpenRead(settings.DeshakerTempLog), Encoding.ASCII)) {
                                string LogLine, LogNum, LogNumField, LineOut;
                                long NewLineNum = SegmentStart;
                                LogLine = sr.ReadLine();
                                while (LogLine != null) {
                                    if (LogLine.Length > 7) {
                                        LogNum = LogLine.Substring(0, 7).Trim();
                                        if (LogNum.Length > 0) {
                                            LogNumField = LogNum[LogNum.Length - 1].ToString();
                                            if (LogNumField != "A" && LogNumField != "B") // For interlaced videos
                                                LogNumField = "";
                                            NewLineNum++; // Log file starts at 1, not 0.
                                            LineOut = (NewLineNum.ToString() + LogNumField).PadLeft(7) + LogLine.Substring(7, LogLine.Length - 8);
                                            sw.WriteLine(LineOut);
                                        }
                                    }
                                    LogLine = sr.ReadLine();
                                }
                            }
                        }
                        File.Delete(settings.DeshakerTempLog);
                    }
                }
            } else
                Result = CompletionStatus.Error;

            // End UI.
            FFmpegConfig.UserInterfaceManager.Stop(JobId);
            return Result;
        }

        private CompletionStatus GenerateDeshakerLogSegment(MediaEncoderSettings settings, string inputFile, int segment, long jobStart, long frameStart, long frameEnd, ProcessStartOptions jobOptions) {
            // Write Deshaker Pass 1 script to file.
            string Script = MediaEncoderScript.GenerateDeshakerScript(settings, inputFile, segment, frameStart, frameEnd);
            File.WriteAllText(settings.DeshakerScript, Script);

            // Run pass.
            jobOptions.IsMainTask = true;
            jobOptions.Title = "Running Deshaker Prescan";
            jobOptions.ResumePos = frameStart - jobStart;
            CompletionStatus Result = MediaEncoder.ConvertToAvi(settings.DeshakerScript, settings.DeshakerTempOut, false, jobOptions);
            File.Delete(settings.DeshakerScript);
            File.Delete(settings.DeshakerTempOut);
            return Result;
        }
    }

    public class EncodingCompletedEventArgs : EventArgs {
        public string OldFileName { get; set; }
        public string NewFileName { get; set; }
        public TimeSpan EncodingTime { get; set; }
        public long OldFileSize { get; set; }
        public long NewFileSize { get; set; }
        public MediaEncoderSettings Settings { get; set; }

        public EncodingCompletedEventArgs() {
        }

        public string OldFileNameShort {
            get {
                return Path.GetFileName(OldFileName);
            }
        }
    }
}
