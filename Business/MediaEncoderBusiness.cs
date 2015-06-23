using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using DataAccess;
using System.Globalization;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Business {
    public class MediaEncoderBusiness {
        private static int JobIndex = 0;
        private static Task<EncodingCompletedEventArgs> EncoderTask = null;
        public static ObservableCollection<string> ProcessingQueue = new ObservableCollection<string>();
        public event EventHandler<EncodingCompletedEventArgs> EncodingCompleted;
        private string PreviewScriptFile = Settings.TempFilesPath + "Preview.avs";
        private string PreviewSettingsFile = Settings.TempFilesPath + "Preview.xml";
        private string PreviewSourceFile = Settings.TempFilesPath + "Preview.avi";

        public void GenerateScript(MediaEncoderSettings settings, bool preview, bool multiThreaded) {
            string Script = GetVideoScript(settings, PreviewSourceFile, preview, multiThreaded);
            WriteFile(Script, PreviewScriptFile);
            SaveSettingsFile(settings, PreviewSettingsFile);
        }

        public async Task DeletePreviewFilesAsync() {
            for (int i = 0; i < 5; i++) {
                try {
                    File.Delete(PreviewSourceFile);
                    File.Delete(PreviewScriptFile);
                    File.Delete(PreviewSettingsFile);
                    return;
                } catch {
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
            SafeMove(PreviewSourceFile, settings.InputFile);
            SaveSettingsFile(settings, settings.SettingsFile);
            string Script = GetVideoScript(settings, settings.InputFile, false, true);
            WriteFile(Script, settings.ScriptFile);

            await StartEncodeFileAsync(settings);
        }

        private async Task StartEncodeFileAsync(MediaEncoderSettings settings) {
            ProcessingQueue.Add(Path.GetFileNameWithoutExtension(settings.FileName));

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
            ProcessingQueue.Add(Path.GetFileNameWithoutExtension(settings.FileName));

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
            FfmpegBusiness.ConvertToH264(settings.ScriptFile, settings.OutputFile, settings.EncodeQuality);
            Result = FinalizeEncoding(settings, StartTime);
            return Result;
        }

        private EncodingCompletedEventArgs FinalizeEncoding(MediaEncoderSettings settings, DateTime? startTime) {
            EncodingCompletedEventArgs Result = null;
            if (File.Exists(settings.OutputFile)) {
                // Muxe video with audio.
                string FinalFile = GetNextAvailableFileName(settings.FinalFile);
                bool CopyAudio = !settings.EncodeMp4 && !settings.Trim && !settings.ChangeSpeed;
                if (CopyAudio)
                    FfmpegBusiness.JoinAudioVideo(settings.OutputFile, Settings.NaturalGroundingFolder + settings.FileName, FinalFile, true);
                else
                    FfmpegBusiness.JoinAudioVideo(settings.OutputFile, null, FinalFile, true);

                Result = GetEncodingResults(settings, FinalFile, startTime);
            }

            return Result;
        }

        private EncodingCompletedEventArgs GetEncodingResults(MediaEncoderSettings settings, string finalFile, DateTime? startTime) {
            // Create encoding result object.
            EncodingCompletedEventArgs Result = null;
            if (File.Exists(finalFile)) {
                Result = new EncodingCompletedEventArgs();
                Result.OldFileName = settings.FileName;
                Result.NewFileName = settings.FinalFile.Substring(Settings.NaturalGroundingFolder.Length);
                if (startTime.HasValue)
                    Result.EncodingTime = DateTime.Now - startTime.Value;
                FileInfo FinalFileInfo = new FileInfo(finalFile);
                Result.NewFileSize = FinalFileInfo.Length;
                FinalFileInfo = new FileInfo(Settings.NaturalGroundingFolder + settings.FileName);
                Result.OldFileSize = FinalFileInfo.Length;
                Result.Settings = settings;
            }
            return Result;
        }

        private string GetVideoScript(MediaEncoderSettings settings, string inputFile, bool preview, bool multiThreaded) {
            int CPU = Environment.ProcessorCount;
            bool AviSynthPlus = false;
            // multiThreaded = true;

            // Calculate CropSource and CropAfter values.
            bool CropSource = false;
            int CropSourceLeft = 0;
            int CropSourceTop = 0;
            int CropSourceRight = 0;
            int CropSourceBottom = 0;
            bool CropAfter = false;
            int CropAfterLeft = 0;
            int CropAfterTop = 0;
            int CropAfterRight = 0;
            int CropAfterBottom = 0;
            if (settings.Crop) {
                // CropSource must be a factor of two.
                CropSourceLeft = settings.CropLeft / 2 * 2;
                CropSourceTop = settings.CropTop / 2 * 2;
                CropSourceRight = settings.CropRight / 2 * 2;
                CropSourceBottom = settings.CropBottom / 2 * 2;
            }
            int ScaleFactor = (settings.DoubleNNEDI3Before ? 2 : 1) * (settings.DoubleEEDI3 ? 2 : 1) * (settings.DoubleNNEDI3 ? 2 : 1);
            int ExtraDouble = 0;
            // If cropping makes the output smaller than the resize dimension, keep doubling to enlarge.
            if (settings.Resize) {
                while ((settings.SourceHeight - CropSourceTop - CropSourceBottom) * ScaleFactor < settings.ResizeHeight) {
                    ExtraDouble += 2;
                    ScaleFactor *= 2;
                }
            }
            if (settings.Crop) {
                CropAfterTop = (settings.CropTop - CropSourceTop) * ScaleFactor;
                CropAfterLeft = (settings.CropLeft - CropSourceLeft) * ScaleFactor;
                CropAfterRight = (settings.CropRight - CropSourceRight) * ScaleFactor;
                CropAfterBottom = (settings.CropBottom - CropSourceBottom) * ScaleFactor;
                CropSource = (CropSourceLeft > 0 || CropSourceRight > 0 || CropSourceTop > 0 || CropSourceBottom > 0);
                CropAfter = (CropAfterLeft > 0 || CropAfterRight > 0 || CropAfterTop > 0 || CropAfterBottom > 0);
            }
            // Calculate encoding and final frame rates.
            double ChangeSpeedValue = settings.ChangeSpeed ? (double)settings.ChangeSpeedValue / 100 : 1;
            double FrameRateBefore = settings.SourceFrameRate.Value * ChangeSpeedValue;

            // Generate video script.
            StringBuilder Result = new StringBuilder();
            if (multiThreaded) {
                //Result.AppendLine("Setmemorymax(512)");
                if (AviSynthPlus) {
                    Result.AppendLine(@"SetFilterMTMode(""DEFAULT_MT_MODE"",2)");
                    Result.AppendLine(@"SetFilterMTMode(""AviSource"",3)");
                } else {
                    Result.AppendLine("SetMTMode(3,4)");
                }
            }
            Result.AppendFormat(@"PluginPath = ""{0}""", GetAsciiPath(Settings.AviSynthPluginsPath)).AppendLine();
            Result.AppendFormat(@"AviSource(""{0}"", audio=false, pixel_type=""YV12"")", GetAsciiPath(inputFile)).AppendLine(); //.ConvertToYV12()
            if (multiThreaded && !AviSynthPlus)
                Result.AppendLine("SetMTMode(2)");
            if (settings.FixColors && !preview) {
                Result.AppendLine(@"LoadPlugin(PluginPath+""ColorMatrix.dll"")");
                Result.AppendLine(@"ColorMatrix(mode=""Rec.601->Rec.709"")");
            }
            if (FrameRateBefore != settings.SourceFrameRate)
                Result.AppendFormat(CultureInfo.InvariantCulture, "AssumeFPS({0})", FrameRateBefore).AppendLine();
            if (settings.Trim) {
                Result.AppendFormat("Trim({0}, {1})",
                    settings.TrimStart.HasValue ? settings.TrimStart.Value * settings.SourceFrameRate.Value : 0,
                    settings.TrimEnd.HasValue && !preview ? settings.TrimEnd.Value * settings.SourceFrameRate.Value : 0).AppendLine();
            }
            if (CropSource) {
                Result.AppendFormat("Crop({0}, {1}, -{2}, -{3})", CropSourceLeft, CropSourceTop, CropSourceRight, CropSourceBottom).AppendLine();
            }
            if (settings.Denoise || settings.SharpenAfterDouble || settings.SharpenFinal)
                Result.AppendLine(@"LoadPlugin(PluginPath+""FFT3DFilter.dll"")");
            if (settings.Denoise)
                Result.AppendFormat(@"fft3dfilter(sigma={0}, bt=5, bw=48, bh=48, ow=24, oh=24, ncpu={1})", ((double)settings.DenoiseStrength / 10).ToString(CultureInfo.InvariantCulture), CPU).AppendLine();
            if (settings.DoubleNNEDI3 || settings.DoubleNNEDI3Before || ExtraDouble > 0)
                Result.AppendLine(@"LoadPlugin(PluginPath+""nnedi3.dll"")");
            if (settings.DoubleNNEDI3Before) {
                Result.AppendLine("nnedi3_rpow2 (2)");
                if (settings.SharpenAfterDouble && (settings.DoubleEEDI3 || (settings.DoubleNNEDI3 || ExtraDouble > 0)))
                    Result.AppendFormat(CultureInfo.InvariantCulture, @"fft3dfilter(bt=-1, sharpen={0}, ncpu={1})", (double)settings.SharpenAfterDoubleStrength / 100, CPU).AppendLine();
            }
            if (settings.DoubleEEDI3) {
                Result.AppendLine(@"LoadPlugin(PluginPath+""eedi3.dll"")");
                Result.AppendLine("eedi3_rpow2 (2)");
                if (settings.SharpenAfterDouble && (settings.DoubleNNEDI3 || ExtraDouble > 0))
                    Result.AppendFormat(CultureInfo.InvariantCulture, @"fft3dfilter(bt=-1, sharpen={0}, ncpu={1})", (double)settings.SharpenAfterDoubleStrength / 100, CPU).AppendLine();
            }
            if (settings.DoubleNNEDI3 || ExtraDouble > 0) {
                Result.AppendFormat("nnedi3_rpow2 ({0})", (settings.DoubleNNEDI3 ? 2 : 0) + ExtraDouble).AppendLine();
            }
            if (settings.Resize || settings.SourceAspectRatio != 1 || CropAfter) {
                // Calculate width for resize.
                if (!settings.Resize) // If pixels are not square, we must always resize to preserve aspect ratio.
                    settings.ResizeHeight = settings.SourceHeight.Value * ScaleFactor;
                int CropHeight = ((settings.SourceHeight.Value - CropSourceTop - CropSourceBottom) * ScaleFactor);
                int CropWidth = ((settings.SourceWidth.Value - CropSourceLeft - CropSourceRight) * ScaleFactor);
                if (CropAfter) {
                    CropHeight = CropHeight - CropAfterTop - CropAfterBottom;
                    CropWidth = CropWidth - CropAfterLeft - CropAfterRight;
                }
                int ResizeWidth = (int)Math.Round((double)CropWidth * settings.SourceAspectRatio / CropHeight * settings.ResizeHeight / 4) * 4;
                if (CropAfter) {
                    Result.AppendFormat("Spline36Resize({0}, {1}, {2}, {3}, -{4}, -{5})",
                        ResizeWidth, settings.ResizeHeight, CropAfterLeft, CropAfterTop, CropAfterRight, CropAfterBottom).AppendLine();
                } else {
                    Result.AppendFormat("Spline36Resize({0}, {1})", ResizeWidth, settings.ResizeHeight).AppendLine();
                }
            }
            if (settings.IncreaseFrameRate) {
                Result.AppendLine(@"LoadPlugin(PluginPath+""svpflow1.dll"")");
                Result.AppendLine(@"LoadPlugin(PluginPath+""svpflow2.dll"")");
                Result.AppendLine(@"Import(PluginPath+""InterFrame2.avsi"")");
                if (settings.IncreaseFrameRateValue == FrameRateModeEnum.Double)
                    Result.AppendFormat(@"InterFrame(Cores={0}, Tuning=""Smooth"", FrameDouble=true{1})", CPU, Settings.SavedFile.EnableMadVR ? ", GPU=true" : "").AppendLine();
                else {
                    int NewNum = 0;
                    int NewDen = 0;
                    if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps30) {
                        NewNum = 30000;
                        NewDen = 1001;
                    } else if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps60) {
                        NewNum = 60000;
                        NewDen = 1001;
                    } else if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps120) {
                        NewNum = 120000;
                        NewDen = 1001;
                    }
                    Result.AppendFormat(@"InterFrame(Cores={0}, Tuning=""Smooth"", NewNum={1}, NewDen={2}{3})", CPU, NewNum, NewDen, Settings.SavedFile.EnableMadVR ? ", GPU=true" : "").AppendLine();
                }
            }
            if (settings.SharpenFinal)
                Result.AppendFormat(CultureInfo.InvariantCulture, @"fft3dfilter(bt=-1, sharpen={0}, ncpu={1})", (double)settings.SharpenFinalStrength / 100, CPU).AppendLine();
            if (preview && (settings.Crop))
                Result.AppendLine("AddBorders(2, 2, 2, 2, $FFFFFF)");
            if (preview)
                Result.AppendLine("ConvertToRGB32()");
            if (multiThreaded) {
                if (AviSynthPlus)
                    Result.AppendLine("Prefetch(4)");
                else
                    Result.AppendLine("Distributor()");
            }
            return Result.ToString();
        }

        private void WriteFile(string content, string fileName) {
            if (!Directory.Exists(Settings.TempFilesPath))
                Directory.CreateDirectory(Settings.TempFilesPath);

            File.WriteAllText(fileName, content, Encoding.ASCII);
        }

        public async Task ConvertToAvi(string fileName, MediaEncoderSettings settings) {
            await Task.Run(() => FfmpegBusiness.ConvertToAVI(Settings.NaturalGroundingFolder + fileName, PreviewSourceFile, false));

            // Get media info.
            MediaInfoReader InfoReader = new MediaInfoReader();
            await InfoReader.LoadInfoAsync(PreviewSourceFile);
            settings.SourceWidth = InfoReader.Width;
            settings.SourceHeight = InfoReader.Height;
            settings.SourceAspectRatio = InfoReader.PixelAspectRatio ?? 1;
            // Fix last track of VCDs that is widescreen.
            if (settings.SourceHeight == 288 && settings.SourceWidth == 352 && settings.SourceAspectRatio == 1.485f)
                settings.SourceAspectRatio = 1.092f;
            settings.SourceFrameRate = InfoReader.FrameRate;
            settings.Position = (InfoReader.Length ?? 0) / 2;
        }

        public async Task FinalizeReplaceAsync(EncodingCompletedEventArgs jobInfo) {
            await Task.Run(() => FinalizeReplace(jobInfo));
        }

        public void FinalizeReplace(EncodingCompletedEventArgs jobInfo) {
            EditVideoBusiness EditBusiness = new EditVideoBusiness();
            Media EditVideo = EditBusiness.GetVideoByFileName(jobInfo.OldFileName);
            System.Threading.Thread.Sleep(200); // Give MPC time to release the file.
            string OriginalPath = Path.Combine(Path.GetDirectoryName(Settings.NaturalGroundingFolder + jobInfo.OldFileName), "Original", Path.GetFileName(jobInfo.OldFileName));
            string NewPath = Path.Combine(Path.GetDirectoryName(Settings.NaturalGroundingFolder + jobInfo.OldFileName), Path.GetFileNameWithoutExtension(jobInfo.OldFileName) + Path.GetExtension(jobInfo.Settings.FinalFile));
            Directory.CreateDirectory(Path.GetDirectoryName(OriginalPath));
            SafeMove(Settings.NaturalGroundingFolder + jobInfo.OldFileName, OriginalPath);
            SafeMove(jobInfo.Settings.FinalFile, NewPath);
            jobInfo.Settings.FileName = OriginalPath.Substring(Settings.NaturalGroundingFolder.Length);

            if (EditVideo != null) {
                EditVideo.FileName = NewPath.Substring(Settings.NaturalGroundingFolder.Length);
                EditVideo.Height = null;
                EditBusiness.Save();
            }
        }

        public void FinalizeKeep(EncodingCompletedEventArgs jobInfo) {
            string FinalFile = String.Format("{0} - Encoded.{1}",
                Path.Combine(Path.GetDirectoryName(Settings.NaturalGroundingFolder + jobInfo.OldFileName),
                    Path.GetFileNameWithoutExtension(jobInfo.OldFileName)),
                    jobInfo.Settings.EncodeMp4 ? "mp4" : "mkv");
            SafeMove(Settings.NaturalGroundingFolder + jobInfo.NewFileName, FinalFile);
        }

        public void DeleteJobFiles(MediaEncoderSettings settings) {
            File.Delete(settings.InputFile);
            File.Delete(settings.OutputFile);
            File.Delete(settings.FinalFile);
            File.Delete(settings.ScriptFile);
            File.Delete(settings.SettingsFile);
        }

        /// <summary>
        /// Moves specified settings file as preview files.
        /// </summary>
        /// <param name="settings">The settings to use for re-encoding.</param>
        public async Task MovePreviewFilesAsync(MediaEncoderSettings settings) {
            await DeletePreviewFilesAsync();
            File.Move(settings.InputFile, PreviewSourceFile);
            SaveSettingsFile(settings, PreviewSettingsFile);
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
                MediaEncoderSettings settings = LoadSettingsFile(PreviewSettingsFile);
                if (!File.Exists(PreviewSourceFile) && File.Exists(Settings.NaturalGroundingFolder + settings.FileName))
                    await ConvertToAvi(settings.FileName, settings);
                return settings;
            }
            return null;
        }

        /// <summary>
        /// Automatically reloads jobs if the encoder was unexpectedly closed.
        /// </summary>
        public async Task AutoLoadJobsAsync() {
            if (!Directory.Exists(Settings.TempFilesPath))
                Directory.CreateDirectory(Settings.TempFilesPath);

            var JobList = Directory.EnumerateFiles(Settings.TempFilesPath, "Job*_Settings.xml");
            int Index = 0;
            MediaEncoderSettings settings;
            List<Task> TaskList = new List<Task>();

            // Get list of interrupted jobs.
            foreach (string item in JobList) {
                // Load settings file.
                settings = null;
                try {
                    settings = LoadSettingsFile(item);
                } catch { }

                // Resume job.
                if (int.TryParse(Path.GetFileName(item).Replace("Job", "").Replace("_Settings.xml", ""), out Index)) {
                    if (Index > JobIndex)
                        JobIndex = Index;

                    if (settings != null && File.Exists(settings.InputFile) && File.Exists(settings.ScriptFile) && File.Exists(settings.SettingsFile) && File.Exists(Settings.NaturalGroundingFolder + settings.FileName)) {
                        if (File.Exists(settings.FinalFile)) {
                            // Merging was completed.
                            EncodingCompletedEventArgs EncodeResult = GetEncodingResults(settings, settings.FinalFile, null);
                            if (EncodeResult != null)
                                EncodingCompleted(this, EncodeResult);
                        } else if (File.Exists(settings.OutputFile)) {
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
            return Process.GetProcessesByName("x264").FirstOrDefault();
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
            } catch (IOException) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            } finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public void SaveSettingsFile(MediaEncoderSettings settings, string fileName) {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (var writer = new StreamWriter(fileName)) {
                var serializer = new XmlSerializer(typeof(MediaEncoderSettings));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                serializer.Serialize(writer, settings, ns);
                writer.Flush();
            }
        }

        public MediaEncoderSettings LoadSettingsFile(string fileName) {
            using (var stream = File.OpenRead(fileName)) {
                var serializer = new XmlSerializer(typeof(MediaEncoderSettings));
                MediaEncoderSettings Result = serializer.Deserialize(stream) as MediaEncoderSettings;
                return Result;
            }
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
                } catch {
                }
                System.Threading.Thread.Sleep(500);
            }
            throw new IOException(string.Format("Cannot move file '{0}' because it is being used by another process.", source));
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

        private static string GetShortPath(string path) {
            const int MAX_PATH = 255;
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }

        public bool IsASCII(string value) {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        private string GetAsciiPath(string path) {
            if (!IsASCII(path))
                return GetShortPath(path);
            else
                return path;
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
                    } catch {
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
