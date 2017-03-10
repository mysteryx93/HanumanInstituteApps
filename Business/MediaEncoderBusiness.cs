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

namespace Business {
    public class MediaEncoderBusiness {
        private static int JobIndex = 0;
        private static Task<EncodingCompletedEventArgs> EncoderTask = null;
        public static ObservableCollection<string> ProcessingQueue = new ObservableCollection<string>();

        public event EventHandler<EncodingCompletedEventArgs> EncodingCompleted;
        static public string PreviewScriptFile {
            get { return Settings.TempFilesPath + "Preview.avs"; }
        }
        static public string PreviewSettingsFile {
            get { return Settings.TempFilesPath + "Preview.xml"; }
        }
        static public string PreviewSourceFile {
            get { return Settings.TempFilesPath + "Preview.avi"; }
        }
        static public string PreviewDeshakerScript {
            get { return Settings.TempFilesPath + "Preview_Deshaker.avs"; }
        }
        static public string PreviewDeshakerLog {
            get { return Settings.TempFilesPath + "Preview_Deshaker.log"; }
        }

        public void GenerateScript(MediaEncoderSettings settings, bool preview, bool multiThreaded) {
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
            if (Script.IsEmpty)
                Script = MediaEncoderScript.GenerateVideoScript(settings, GetPreviewSourceFile(settings), preview, multiThreaded);
            else if (preview) {
                Script.RemoveMT();
                Script.AppendLine(@"ConvertToRGB32(matrix=""Rec709"")");
            } else if (!multiThreaded)
                Script.RemoveMT();
            Script.WriteToFile(PreviewScriptFile);
            settings.Save(PreviewSettingsFile);
        }

        public void GenerateCustomScript(MediaEncoderSettings settings) {
            settings.CustomScript = MediaEncoderScript.GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true).Script;
        }

        public bool CustomScriptHasChanges(MediaEncoderSettings settings) {
            return settings.CustomScript.Replace("\r\n", "\n") != MediaEncoderScript.GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true).Script.Replace("\r\n", "\n");
        }

        public string GetPreviewSourceFile(MediaEncoderSettings settings) {
            if (settings.ConvertToAvi)
                return PreviewSourceFile;
            else
                return settings.FilePath;
        }

        public async Task DeletePreviewFilesAsync() {
            for (int i = 0; i < 5; i++) {
                try {
                    File.Delete(PreviewSourceFile);
                    File.Delete(PreviewScriptFile);
                    File.Delete(PreviewSettingsFile);
                    File.Delete(PreviewDeshakerScript);
                    File.Delete(PreviewDeshakerLog);
                    return;
                }
                catch {
                }
                await Task.Delay(200);
            }
        }

        /// <summary>
        /// Starts encoding with specified settings. If an encoding is already in process, it will be added
        /// to the queue and start once the previous encodings are finished.
        /// </summary>
        /// <param name="settings">The encoding settings.</param>
        public async Task EncodeFileAsync(MediaEncoderSettings settings) {
            settings.JobIndex = ++JobIndex;
            // Files must be prepared before adding to queue so that user can replace preview files.
            DeleteJobFiles(settings);
            File.Delete(PreviewScriptFile);
            File.Delete(PreviewSettingsFile);
            if (settings.ConvertToAvi)
                SafeMove(PreviewSourceFile, settings.InputFile);
            if (settings.Deshaker)
                SafeMove(PreviewDeshakerLog, settings.DeshakerLog);
            settings.Save(settings.SettingsFile);
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
            if (Script.IsEmpty)
                Script = MediaEncoderScript.GenerateVideoScript(settings, settings.InputFile, false, true);
            else {
                Script.Replace(Script.GetAsciiPath(PreviewSourceFile), Script.GetAsciiPath(settings.InputFile));
                Script.Replace(Script.GetAsciiPath(PreviewDeshakerLog), Script.GetAsciiPath(settings.DeshakerLog));
            }
            //Script.DitherOut(true);
            Script.WriteToFile(settings.ScriptFile);
            await StartEncodeFileAsync(settings);
        }

        private async Task StartEncodeFileAsync(MediaEncoderSettings settings) {
            ProcessingQueue.Add(Path.GetFileNameWithoutExtension(settings.FilePath));

            EncodingCompletedEventArgs EncodingResult;
            if (EncoderTask == null)
                EncoderTask = Task.Run(() => EncodeFileThread(settings));
            else
                EncoderTask = EncoderTask.ContinueWith((prevTask) => EncodeFileThread(settings));
            EncodingResult = await EncoderTask;

            ProcessingQueue.RemoveAt(0);
            if (EncodingResult != null && EncodingCompleted != null)
                EncodingCompleted(this, EncodingResult);
        }

        private async Task WaitEncodeProcessAsync(MediaEncoderSettings settings, Process jobProcess) {
            ProcessingQueue.Add(Path.GetFileNameWithoutExtension(settings.FilePath));

            EncodingCompletedEventArgs EncodingResult;
            if (EncoderTask == null)
                EncoderTask = Task.Run(() => {
                    jobProcess.WaitForExit();
                    return FinalizeEncoding(settings, DateTime.Now);
                });
            else
                EncoderTask = EncoderTask.ContinueWith((prevTask) => {
                    jobProcess.WaitForExit();
                    return FinalizeEncoding(settings, DateTime.Now);
                });
            EncodingResult = await EncoderTask;

            ProcessingQueue.RemoveAt(0);
            if (EncodingResult != null && EncodingCompleted != null)
                EncodingCompleted(this, EncodingResult);
        }

        private EncodingCompletedEventArgs EncodeFileThread(MediaEncoderSettings settings) {
            EncodingCompletedEventArgs Result = null;
            DateTime StartTime = DateTime.Now;
            Task<CompletionStatus> VideoEnc = null;
            if (settings.VideoCodec != VideoCodecs.Copy)
                VideoEnc = Task.Run(() => AvisynthTools.EncodeVideo(settings));

            // Encode audio stream
            if (settings.IsAudioEncode) {
                Task<CompletionStatus> AudioEnc = Task.Run(() => AvisynthTools.EncodeAudio(settings));
                if (VideoEnc != null)
                    Task.WaitAll(VideoEnc, AudioEnc);
                else
                    Task.WaitAll(AudioEnc);
            } else if (VideoEnc != null)
                Task.WaitAll(VideoEnc);

            Result = FinalizeEncoding(settings, StartTime);
            return Result;
        }


        private EncodingCompletedEventArgs FinalizeEncoding(MediaEncoderSettings settings, DateTime? startTime) {
            EncodingCompletedEventArgs Result = null;
            string FinalFile = GetNextAvailableFileName(settings.FinalFile);
            string VideoFile = null;
            if (settings.VideoCodec == VideoCodecs.Copy)
                VideoFile = settings.FilePath;
            else
                VideoFile = settings.OutputFile;

            if (settings.VideoCodec == VideoCodecs.Avi) {
                File.Move(VideoFile, FinalFile);
            } else {
                // Muxe video with audio.
                string AudioFile = null;
                if (settings.AudioAction == AudioActions.Copy)
                    AudioFile = settings.FilePath;
                else if (settings.AudioAction == AudioActions.EncodeWav)
                    AudioFile = settings.AudioFileWav;
                else if (settings.AudioAction == AudioActions.EncodeFlac)
                    AudioFile = settings.AudioFileFlac;
                else if (settings.AudioAction == AudioActions.EncodeAac)
                    AudioFile = settings.AudioFileAac;
                else if (settings.AudioAction == AudioActions.EncodeOpus)
                    AudioFile = settings.AudioFileOpus;
                MediaMuxer.Muxe(VideoFile, AudioFile, FinalFile, new ProcessStartOptions(FFmpegDisplayMode.ErrorOnly, "Muxing Audio and Video"));
            }
            Result = GetEncodingResults(settings, FinalFile, startTime);

            return Result;
        }

        private EncodingCompletedEventArgs GetEncodingResults(MediaEncoderSettings settings, string finalFile, DateTime? startTime) {
            // Create encoding result object.
            EncodingCompletedEventArgs Result = null;
            if (File.Exists(finalFile)) {
                Result = new EncodingCompletedEventArgs();
                Result.OldFileName = settings.FilePath;
                Result.NewFileName = settings.FinalFile;
                if (startTime.HasValue)
                    Result.EncodingTime = DateTime.Now - startTime.Value;
                FileInfo FinalFileInfo = new FileInfo(finalFile);
                Result.NewFileSize = FinalFileInfo.Length;
                FinalFileInfo = new FileInfo(settings.FilePath);
                Result.OldFileSize = FinalFileInfo.Length;
                Result.Settings = settings;
            }
            return Result;
        }



        public async Task PreparePreviewFile(MediaEncoderSettings settings, bool overwrite, bool calcAutoCrop) {
            if (string.IsNullOrEmpty(settings.FilePath))
                return;

            if (overwrite) {
                File.Delete(PreviewSourceFile);
                // Select default open method.
                if (settings.FilePath.ToLower().EndsWith(".avi"))
                    settings.ConvertToAvi = false;
                else {
                    using (MediaInfoReader InfoReader = new MediaInfoReader()) {
                        InfoReader.LoadInfo(settings.FilePath);
                        if (settings.ConvertToAvi && InfoReader.Height.HasValue && InfoReader.Height >= 720)
                            settings.ConvertToAvi = false;
                    }
                }
            }

            bool AviFileReady = File.Exists(PreviewSourceFile);
            if (!AviFileReady && settings.ConvertToAvi)
                AviFileReady = await Task.Run(() => MediaEncoder.ConvertToAvi(settings.FilePath, PreviewSourceFile, new ProcessStartOptions(FFmpegDisplayMode.Interface, "Converting to AVI"))) == CompletionStatus.Success;

            if (AviFileReady && settings.ConvertToAvi)
                await GetMediaInfo(PreviewSourceFile, settings);
            else {
                settings.ConvertToAvi = false;
                await GetMediaInfo(settings.FilePath, settings);
            }

            // Auto-calculate crop settings.
            if (calcAutoCrop) {
                if (settings.CropLeft == 0 && settings.CropTop == 0 && settings.CropRight == 0 && settings.CropBottom == 0) {
                    Rect AutoCrop = await Task.Run(() => AvisynthTools.GetAutoCropRect(settings));
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

        private async Task GetMediaInfo(string previewFile, MediaEncoderSettings settings) {
            FFmpegProcess Worker = await Task.Run(() => MediaMuxer.GetFileInfo(previewFile));
            FFmpegVideoStreamInfo VInfo = Worker.VideoStream;
            FFmpegAudioStreamInfo AInfo = settings.ConvertToAvi ? await Task.Run(() => MediaMuxer.GetFileInfo(settings.FilePath).AudioStream) : Worker.AudioStream;

            settings.SourceWidth = Worker.VideoStream.Width;
            settings.SourceHeight = Worker.VideoStream.Height;
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
            if (!settings.CanCopyAudio && settings.AudioAction == AudioActions.Copy)
                settings.EncodeFormat = VideoFormats.Mkv;
            settings.SourceAudioBitrate = AInfo.Bitrate;
            if (!settings.IsAudioEncode)
                settings.AudioQuality = AInfo.Bitrate > 0 ? AInfo.Bitrate : 256;
            if (settings.AudioQuality > 384)
                settings.AudioQuality = 384;
            settings.SourceBitDepth = 8; // VInfo.BitDepth;
            settings.DenoiseD = 2;
            settings.DenoiseA = settings.SourceHeight < 720 ? 2 : 1;
            settings.Position = Worker.FileDuration.TotalSeconds / 2;
            // Use Cache to open file when file is over 500MB
            settings.CalculateSize();
        }

        public void FinalizeReplace(EncodingCompletedEventArgs jobInfo) {
            EditVideoBusiness EditBusiness = new EditVideoBusiness();
            string RelativePath = jobInfo.OldFileName.StartsWith(Settings.NaturalGroundingFolder) ? jobInfo.OldFileName.Substring(Settings.NaturalGroundingFolder.Length) : jobInfo.OldFileName;
            Media EditVideo = EditBusiness.GetVideoByFileName(RelativePath);
            System.Threading.Thread.Sleep(200); // Give MPC time to release the file.
            string OriginalPath = Path.Combine(Path.GetDirectoryName(jobInfo.OldFileName), "Original", Path.GetFileName(jobInfo.OldFileName));
            string NewPath = Path.Combine(Path.GetDirectoryName(jobInfo.OldFileName), Path.GetFileNameWithoutExtension(jobInfo.OldFileName) + Path.GetExtension(jobInfo.Settings.FinalFile));
            Directory.CreateDirectory(Path.GetDirectoryName(OriginalPath));
            SafeMove(jobInfo.OldFileName, OriginalPath);
            SafeMove(jobInfo.Settings.FinalFile, NewPath);
            jobInfo.Settings.FilePath = OriginalPath.Substring(Settings.NaturalGroundingFolder.Length);

            if (EditVideo != null) {
                EditVideo.FileName = NewPath.Substring(Settings.NaturalGroundingFolder.Length);
                EditVideo.Height = null;
                EditBusiness.Save();
            }
        }

        public void FinalizeKeep(EncodingCompletedEventArgs jobInfo) {
            string FinalFile = String.Format("{0} - Encoded.{1}",
                Path.Combine(Path.GetDirectoryName(jobInfo.OldFileName),
                    Path.GetFileNameWithoutExtension(jobInfo.OldFileName)),
                    jobInfo.Settings.FileExtension);
            SafeMove(jobInfo.NewFileName, FinalFile);
        }

        public void DeleteJobFiles(MediaEncoderSettings settings) {
            if (settings.ConvertToAvi)
                File.Delete(settings.InputFile);
            File.Delete(settings.OutputFile);
            File.Delete(settings.AudioFileWav);
            File.Delete(settings.AudioFileAac);
            File.Delete(settings.AudioFileOpus);
            File.Delete(settings.AudioFileFlac);
            File.Delete(settings.FinalFile);
            File.Delete(settings.ScriptFile);
            File.Delete(settings.SettingsFile);
            File.Delete(settings.DeshakerScript);
            File.Delete(settings.DeshakerLog);
        }

        /// <summary>
        /// Moves specified settings file as preview files.
        /// </summary>
        /// <param name="settings">The settings to use for re-encoding.</param>
        public async Task MovePreviewFilesAsync(MediaEncoderSettings settings) {
            await DeletePreviewFilesAsync();
            if (settings.ConvertToAvi)
                File.Move(settings.InputFile, PreviewSourceFile);
            if (settings.Deshaker && File.Exists(settings.DeshakerLog))
                File.Move(settings.DeshakerLog, PreviewDeshakerLog);
            if (!string.IsNullOrEmpty(settings.CustomScript) && (settings.ConvertToAvi || settings.Deshaker)) {
                AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
                if (settings.ConvertToAvi)
                    Script.Replace(Script.GetAsciiPath(settings.InputFile), Script.GetAsciiPath(PreviewSourceFile));
                if (settings.Deshaker)
                    Script.Replace(Script.GetAsciiPath(settings.DeshakerLog), Script.GetAsciiPath(PreviewDeshakerLog));
                settings.CustomScript = Script.Script;
            }
            settings.Save(PreviewSettingsFile);
        }

        /// <summary>
        /// Returns the next available file name to avoid overriding an existing file.
        /// </summary>
        /// <param name="dest">The attempted destination.</param>
        /// <returns>The next available file name.</returns>
        public string GetNextAvailableFileName(string dest) {
            int DuplicateIndex = 0;
            string DestFile;
            do {
                DuplicateIndex++;
                DestFile = string.Format("{0}{1}{2}",
                    Path.Combine(Path.GetDirectoryName(dest),
                        Path.GetFileNameWithoutExtension(dest)),
                    DuplicateIndex > 1 ? string.Format(" ({0})", DuplicateIndex) : "",
                    Path.GetExtension(dest));
            } while (File.Exists(DestFile));
            return DestFile;
        }

        /// <summary>
        /// Auto-load Preview file if encoded was unexpectedly closed.
        /// </summary>
        /// <returns>The previous preview encoding settings.</returns>
        public async Task<MediaEncoderSettings> AutoLoadPreviewFileAsync() {
            if (File.Exists(PreviewSettingsFile)) {
                MediaEncoderSettings settings = MediaEncoderSettings.Load(PreviewSettingsFile);
                if (!File.Exists(PreviewSourceFile) && File.Exists(settings.FilePath)) {
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
        public async Task AutoLoadJobsAsync() {
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
            List<Task> TaskList = new List<Task>();

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
                        if (File.Exists(settings.FinalFile)) {
                            // Merging was completed.
                            EncodingCompletedEventArgs EncodeResult = GetEncodingResults(settings, settings.FinalFile, null);
                            if (EncodeResult != null)
                                EncodingCompleted(this, EncodeResult);
                        } else if (File.Exists(settings.OutputFile) && new FileInfo(settings.OutputFile).Length > 10000) {
                            if (!IsFileLocked(settings.OutputFile)) {
                                // Encoding was completed
                                EncodingCompletedEventArgs EncodeResult = FinalizeEncoding(settings, null);
                                if (EncodeResult != null)
                                    EncodingCompleted(this, EncodeResult);
                                else {
                                    DeleteJobFiles(settings);
                                }
                            } else {
                                // Encoding in progress.
                                Process JobProcess = Getx264Process();
                                if (JobProcess != null)
                                    TaskList.Add(WaitEncodeProcessAsync(settings, JobProcess));
                            }
                        } else {
                            // Encoding had not yet started.
                            File.Delete(settings.OutputFile);
                            TaskList.Add(StartEncodeFileAsync(settings));
                        }
                    } else {
                        // Resume job failed, delete files.
                        DeleteJobFiles(settings);
                    }
                }
            }
            await Task.WhenAll(TaskList);
        }

        private Process Getx264Process() {
            string ProcessName = Path.GetFileNameWithoutExtension(FFmpegConfig.FFmpegPath);
            return Process.GetProcessesByName(ProcessName).FirstOrDefault();
        }

        /// <summary>
        /// Returns whether specified file is in use.
        /// </summary>
        /// <param name="file">The file to check for access.</param>
        /// <returns>True if file is locked, otherwise false.</returns>
        private bool IsFileLocked(string fileName) {
            FileStream stream = null;

            try {
                stream = (new FileInfo(fileName)).Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// Moves specified file to specified destination, numerating the destination to avoid duplicates and attempting several times.
        /// </summary>
        /// <param name="source">The file to move.</param>
        /// <param name="dest">The destination to move the file to.</param>
        /// <returns>The file name being moved to.</returns>
        public string SafeMove(string source, string dest) {
            string DestFile = GetNextAvailableFileName(dest);
            // Attempt to copy file until it becomes available.
            for (int i = 0; i < 5; i++) {
                try {
                    File.Move(source, DestFile);
                    return DestFile;
                }
                catch {
                }
                System.Threading.Thread.Sleep(500);
            }
            throw new IOException(string.Format("Cannot move file '{0}' because it is being used by another process.", source));
        }

        /// <summary>
        /// Clears the temp folder (unfinished downloads) except Media Encoder files.
        /// </summary>
        public static void ClearTempFolder() {
            if (Settings.SavedFile == null || !Directory.Exists(Settings.TempFilesPath))
                return;

            string FileName;
            foreach (string item in Directory.EnumerateFiles(Settings.TempFilesPath)) {
                FileName = Path.GetFileName(item);
                if (!FileName.StartsWith("Preview.") && !FileName.StartsWith("Job")) {
                    try {
                        File.Delete(item);
                    }
                    catch {
                    }
                }
            }
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
    }
}
