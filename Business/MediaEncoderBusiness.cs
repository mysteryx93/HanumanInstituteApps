using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using DataAccess;
using System.Xml;

namespace Business {
    public class MediaEncoderBusiness {
        private static int JobIndex = 0;
        private static Task<EncodingCompletedEventArgs> EncoderTask = null;
        public static ObservableCollection<string> ProcessingQueue = new ObservableCollection<string>();
        private static SupportedOpenClVersion gpuSupportsOpenCL12;

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

        public void GenerateScript(MediaEncoderSettings settings, bool preview, bool multiThreaded) {
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
            if (Script.IsEmpty)
                Script = GenerateVideoScript(settings, GetPreviewSourceFile(settings), preview, multiThreaded);
            else if (preview) {
                Script.RemoveMT();
                Script.AppendLine(@"ConvertToRGB32(matrix=""Rec709"")");
            } else if (!multiThreaded)
                Script.RemoveMT();
            Script.WriteToFile(PreviewScriptFile);
            SaveSettingsFile(settings, PreviewSettingsFile);
        }

        public void GenerateCustomScript(MediaEncoderSettings settings) {
            settings.CustomScript = GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true).Script;
        }

        public bool CustomScriptHasChanges(MediaEncoderSettings settings) {
            return settings.CustomScript.Replace("\r\n", "\n") != GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true).Script.Replace("\r\n", "\n");
        }

        public string GetPreviewSourceFile(MediaEncoderSettings settings) {
            if (settings.ConvertToAvi)
                return PreviewSourceFile;
            else
                return Settings.NaturalGroundingFolder + settings.FileName;
        }

        public async Task DeletePreviewFilesAsync() {
            for (int i = 0; i < 5; i++) {
                try {
                    File.Delete(PreviewSourceFile);
                    File.Delete(PreviewScriptFile);
                    File.Delete(PreviewSettingsFile);
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
            SaveSettingsFile(settings, settings.SettingsFile);
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder(settings.CustomScript);
            if (Script.IsEmpty)
                Script = GenerateVideoScript(settings, settings.InputFile, false, true);
            else
                Script.Replace(Script.GetAsciiPath(PreviewSourceFile), Script.GetAsciiPath(settings.InputFile));
            //Script.DitherOut(true);
            Script.WriteToFile(settings.ScriptFile);
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
            Task<bool> VideoEnc = null;
            if (settings.VideoCodec != VideoCodecs.Copy)
                VideoEnc = Task.Run(() => FfmpegBusiness.EncodeH264(settings));

            // Encode audio stream with Nero Aac Encoder
            if (settings.AudioAction == AudioActions.Encode) {
                Task<bool> AudioEnc = Task.Run(() => EncodeAudio(settings));
                if (VideoEnc != null)
                    Task.WaitAll(VideoEnc, AudioEnc);
                else
                    Task.WaitAll(AudioEnc);
            } else if (VideoEnc != null)
                Task.WaitAll(VideoEnc);

            Result = FinalizeEncoding(settings, StartTime);
            return Result;
        }

        public bool EncodeAudio(MediaEncoderSettings settings) {
            FfmpegBusiness.SaveAudioToWav(settings, true);
            return FfmpegBusiness.EncodeAudio(settings, true);
        }

        private EncodingCompletedEventArgs FinalizeEncoding(MediaEncoderSettings settings, DateTime? startTime) {
            EncodingCompletedEventArgs Result = null;
            if (File.Exists(settings.OutputFile) || (settings.VideoCodec == VideoCodecs.Copy && File.Exists(settings.AudioFileAac))) {
                // Muxe video with audio.
                string FinalFile = GetNextAvailableFileName(settings.FinalFile);
                string VideoFile = null;
                if (settings.VideoCodec == VideoCodecs.Copy)
                    VideoFile = Settings.NaturalGroundingFolder + settings.FileName;
                else
                    VideoFile = settings.OutputFile;
                string AudioFile = null;
                if (settings.AudioAction == AudioActions.Copy)
                    AudioFile = Settings.NaturalGroundingFolder + settings.FileName;
                else if (settings.AudioAction == AudioActions.Encode)
                    AudioFile = settings.AudioFileAac;
                FfmpegBusiness.JoinAudioVideo(VideoFile, AudioFile, FinalFile, true);
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

        public AviSynthScriptBuilder GenerateVideoScript(MediaEncoderSettings settings, string inputFile, bool preview, bool multiThreaded) {
            int CPU = Environment.ProcessorCount;
            if (CPU <= 1)
                multiThreaded = false;
            bool AviSynthPlus = (Settings.EncoderAviSynthVersion == AviSynthVersion.AviSynthPlus);
            // multiThreaded = true;

            settings.CalculateSize();

            // Calculate encoding and final frame rates.
            double ChangeSpeedValue = settings.ChangeSpeed ? (double)settings.ChangeSpeedValue / 100 : 1;
            double FrameRateBefore = settings.SourceFrameRate.Value * ChangeSpeedValue;

            // Generate video script.
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            bool IsYV24 = false;
            Script.AppendLine(); // After clean-up, this adds a line after LoadPlugin commands.
            Script.AddPluginPath();
            if (multiThreaded) {
                // Script.AppendLine("Setmemorymax(1500)");
                if (AviSynthPlus) {
                    Script.LoadPluginAvsi("AviSynthMT.avsi");
                    //Script.AppendLine(@"SetFilterMTMode(""DEFAULT_MT_MODE"",2)");
                    //Script.AppendLine(@"SetFilterMTMode(""LWLibavVideoSource"",3)");
                    //Script.AppendLine(@"SetFilterMTMode(""LWLibavAudioSource"",3)");
                    //if (settings.Denoise1)
                    //    Script.AppendLine(@"SetFilterMTMode(""KNLMeansCL"",3)");
                } else {
                    Script.AppendLine("SetMTMode(3,{0})", CPU);
                }
            }
            if (settings.ConvertToAvi || inputFile.ToLower().EndsWith(".avi"))
                Script.OpenAvi(inputFile, !string.IsNullOrEmpty(settings.SourceAudioFormat));
            else
                Script.OpenDirect(inputFile, null, !string.IsNullOrEmpty(settings.SourceAudioFormat), 1);
            if (FrameRateBefore != settings.SourceFrameRate)
                Script.AppendLine(CultureInfo.InvariantCulture, "AssumeFPS({0}, true)", FrameRateBefore);
            if (settings.Trim) {
                Script.AppendLine("Trim({0}, {1})",
                    (settings.TrimStart ?? 0) > 0 ? (int)(settings.TrimStart.Value * settings.SourceFrameRate.Value) : 0,
                    (settings.TrimEnd ?? 0) > 0 && !preview ? (int)(settings.TrimEnd.Value * settings.SourceFrameRate.Value) : 0);
            }
            if (settings.CropSource.HasValue) {
                Script.AppendLine("Crop({0}, {1}, -{2}, -{3})", settings.CropSource.Left, settings.CropSource.Top, settings.CropSource.Right, settings.CropSource.Bottom);
            }
            if (settings.Denoise1) {
                if (GpuSupport == SupportedOpenClVersion.v12)
                    Script.LoadPluginDll("KNLMeansCL.dll");
                else
                    Script.LoadPluginDll("KNLMeansCL-6.11.dll");
                Script.AppendLine("ConvertToYV24()");
                IsYV24 = true;
                if (multiThreaded && !AviSynthPlus)
                    Script.AppendLine("SetMTMode(5)");
                Script.AppendLine(CultureInfo.InvariantCulture, @"KNLMeansCL(D=1, A=2, h={0}, cmode=true, device_type=""{1}""{2})", 
                    ((double)settings.Denoise1Strength / 10).ToString(CultureInfo.InvariantCulture), 
                    GpuSupport != SupportedOpenClVersion.None ? "GPU" : "CPU",
                    GpuSupport != SupportedOpenClVersion.None ? ", device_id=" + Settings.SavedFile.GraphicDeviceId.ToString() : "");
            }
            if (multiThreaded && !AviSynthPlus)
                Script.AppendLine("SetMTMode(2)");
            if (settings.FrameDouble > 0 || settings.SourceColorMatrix == ColorMatrix.Rec601) {
                Script.LoadPluginDll("Shaders\\Shader.dll");
                Script.LoadPluginAvsi("Shaders\\Shader.avsi");
            }
            bool HasNoShader = (!settings.SuperRes && settings.UpscaleMethod == UpscaleMethods.NNedi3 && settings.DownscaleMethod == DownscaleMethods.Bicubic);
            if (settings.SourceColorMatrix == ColorMatrix.Rec601 && settings.FrameDouble == 0 || HasNoShader) {
                Script.AppendLine("ColorMatrixShader(MatrixIn=\"601\")");
            }
            if (settings.Denoise2) {
                Script.LoadPluginDll("FFT3DFilter.dll");
                Script.AppendLine(CultureInfo.InvariantCulture, @"fft3dfilter(sigma={0}, bt=5, bw=48, bh=48, ow=24, oh=24, sharpen={1})",
                    ((double)settings.Denoise2Strength / 10).ToString(CultureInfo.InvariantCulture),
                    (double)settings.Denoise2Sharpen / 100);
            }

            if (settings.FrameDouble > 0) {
                string FinalSize = string.Format("fWidth={0}, fHeight={1}",
                    settings.OutputWidth + settings.CropAfter.Left + settings.CropAfter.Right,
                    settings.OutputHeight + settings.CropAfter.Top + settings.CropAfter.Bottom);
                string FinalResize = FinalSize;
                if (settings.DownscaleMethod == DownscaleMethods.SSim)
                    FinalResize += string.Format(@", fKernel=""SSim"", fB={0}{1}",
                        settings.SSimStrength / 100, settings.SSimSoft ? ", fC=1" : "");
                else // DownscaleMethod == Bicubic
                    FinalResize += string.Format(@", fKernel=""Bicubic"", fB=0, fC=.75");

                bool IsLastDouble = false;
                if (settings.UpscaleMethod == UpscaleMethods.NNedi3) {
                    if (IsYV24) {
                        Script.AppendLine("ConvertToYV12()");
                        IsYV24 = false;
                    }
                    Script.LoadPluginDll("nnedi3.dll");
                    //Script.LoadPluginDll("FTurn.dll");
                    Script.LoadPluginAvsi("edi_rpow2.avsi");

                    for (int i = 0; i < settings.FrameDouble; i++) {
                        IsLastDouble = i == settings.FrameDouble - 1;
                        string DoubleScript = string.Format("edi_rpow2(2, nns=4, {0}, Threads=2)",
                            IsLastDouble && settings.DownscaleMethod == DownscaleMethods.Bicubic ? @"cshift=""Bicubic"", a1=0, a2=.75, " + FinalSize : @"cshift=""Spline16Resize""");
                        if (settings.SuperRes) {
                            Script.AppendLine(CultureInfo.InvariantCulture, @"SuperRes({0}, {1}, {2}, """"""{3}""""""{4}{5})",
                                settings.SuperRes3Passes ? (i == 0 && settings.FrameDouble > 1 ? 5 : 3) : 2, 
                                settings.SuperResStrength / 100f, settings.SuperResSoftness / 100f, DoubleScript,
                                i == 0 && settings.SourceColorMatrix == ColorMatrix.Rec601 ? ", MatrixIn=\"601\"" : "",
                                IsLastDouble && settings.DownscaleMethod == DownscaleMethods.SSim ? ", " + FinalResize : "");
                        } else {
                            Script.AppendLine(DoubleScript);
                            if (IsLastDouble && settings.DownscaleMethod == DownscaleMethods.SSim) {
                                Script.AppendLine("SSimDownscaler({0}{1})", 
                                    FinalResize.Replace(" f", " ").Replace("fW", "W"), // Use same string as FinalResize but remove 'f' parameter sufix.
                                    settings.SourceColorMatrix == ColorMatrix.Rec601 ? ", MatrixIn=\"601\"" : "");
                            }
                        }
                        if (i == 0 && settings.IncreaseFrameRate)
                            ApplyInterFrame(Script, settings, CPU);
                    }
                } else { // UpscaleMethod == SuperXBR
                    for (int i = 0; i < settings.FrameDouble; i++) {
                        IsLastDouble = i == settings.FrameDouble - 1;
                        if (settings.SuperRes) {
                            Script.AppendLine(CultureInfo.InvariantCulture, @"SuperResXBR({0}, {1}, {2}, XbrStr={3}, XbrSharp={4}{5}{6})",
                                settings.SuperRes3Passes ? (i == 0 && settings.FrameDouble > 1 ? 5 : 3) : 2,
                                settings.SuperResStrength / 100f, settings.SuperResSoftness / 100f,
                                settings.SuperXbrStrength / 10f, settings.SuperXbrSharpness / 10f,
                                i == 0 && settings.SourceColorMatrix == ColorMatrix.Rec601 ? ", MatrixIn=\"601\"" : "",
                                IsLastDouble ? ", " + FinalResize : "");
                        } else {
                            Script.AppendLine("SuperXBR(Str={0}, Sharp={1}{2}{3})",
                                settings.SuperXbrStrength / 10f, settings.SuperXbrSharpness / 10f,
                                i == 0 && settings.SourceColorMatrix == ColorMatrix.Rec601 ? ", MatrixIn=\"601\"" : "",
                                IsLastDouble ? ", " + FinalResize : "");
                        }
                        if (i == 0 && settings.IncreaseFrameRate) {
                            if (IsYV24) {
                                Script.AppendLine("ConvertToYV12()");
                                IsYV24 = false;
                            }
                            ApplyInterFrame(Script, settings, CPU);
                        }
                    }
                }
            }
            if (IsYV24) {
                Script.AppendLine("ConvertToYV12()");
                IsYV24 = false;
            }
            if (settings.FrameDouble == 0 && settings.IncreaseFrameRate)
                ApplyInterFrame(Script, settings, CPU);

            Script.LoadPluginAvsi("ResizeX.avsi");
            if (settings.CropAfter.HasValue || settings.FrameDouble == 0) {
                bool ApplyResize = settings.FrameDouble == 0 || HasNoShader;
                string CropFormat = "ResizeX";
                CropFormat += settings.CropAfter.HasValue ? "({0}, {1}, {2}, {3}, -{4}, -{5}{6})" : "({0}, {1}{6})";
                Script.AppendLine(CropFormat, settings.OutputWidth, settings.OutputHeight,
                    settings.CropAfter.Left, settings.CropAfter.Top, settings.CropAfter.Right, settings.CropAfter.Bottom,
                    ApplyResize ? @", kernel=""Bicubic"", a1=0, a2=.75" : "");
            }

            if (preview) {
                if (settings.Crop)
                    Script.AppendLine("AddBorders(2, 2, 2, 2, $FFFFFF)");
                Script.AppendLine(@"ConvertToRGB32(matrix=""Rec709"")");
            }
            if (multiThreaded && AviSynthPlus)
                Script.AppendLine("Prefetch({0})", CPU);

            Script.Cleanup();
            return Script;
        }

        public void ApplyInterFrame(AviSynthScriptBuilder Script, MediaEncoderSettings settings, int CPU) {
            Script.LoadPluginDll("svpflow1.dll");
            Script.LoadPluginDll("svpflow2.dll");
            Script.LoadPluginAvsi("InterFrame2.avsi");
            if (settings.IncreaseFrameRateValue == FrameRateModeEnum.Double)
                Script.AppendLine(@"InterFrame(Cores={0}{1}, FrameDouble=true{2})", CPU, 
                    settings.IncreaseFrameRateSmooth ? @", Tuning=""Smooth""" : "",
                    Settings.SavedFile.EnableMadVR ? ", GPU=true" : "");
            else {
                int NewNum = 0;
                int NewDen = 0;
                if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps30) {
                    NewNum = 30; // 30000;
                    NewDen = 1; //  1001;
                } else if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps60) {
                    NewNum = 60; // 60000;
                    NewDen = 1; // 1001;
                } else if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps120) {
                    NewNum = 120; // 120000;
                    NewDen = 1; // 1001;
                }
                Script.AppendLine(@"InterFrame(Cores={0}{1}, NewNum={2}, NewDen={3}, GPU=true)", CPU, 
                    settings.IncreaseFrameRateSmooth ? @", Tuning=""Smooth""" : "",
                    NewNum, NewDen);
            }
        }
        
        public async Task PreparePreviewFile(MediaEncoderSettings settings, bool overwrite) {
            if (string.IsNullOrEmpty(settings.FileName))
                return;

            if (overwrite) {
                File.Delete(PreviewSourceFile);
                // Select default open method.
                if (settings.FileName.ToLower().EndsWith(".avi"))
                    settings.ConvertToAvi = false;
                else {
                    using (MediaInfoReader InfoReader = new MediaInfoReader()) {
                        InfoReader.LoadInfo(Settings.NaturalGroundingFolder + settings.FileName);
                        if (settings.ConvertToAvi && InfoReader.Height.HasValue && InfoReader.Height >= 720)
                            settings.ConvertToAvi = false;
                    }
                }
            }

            bool AviFileReady = File.Exists(PreviewSourceFile);
            if (!AviFileReady && settings.ConvertToAvi) 
                AviFileReady = await Task.Run(() => FfmpegBusiness.ConvertToAVI(Settings.NaturalGroundingFolder + settings.FileName, PreviewSourceFile, false));

            if (AviFileReady && settings.ConvertToAvi)
                await GetMediaInfo(PreviewSourceFile, settings);
            else {
                settings.ConvertToAvi = false;
                await GetMediaInfo(Settings.NaturalGroundingFolder + settings.FileName, settings);
            }

            // Auto-calculate crop settings.
            if (settings.CropLeft == 0 && settings.CropTop == 0 && settings.CropRight == 0 && settings.CropBottom == 0) {
                Rect AutoCrop = await Task.Run(() => FfmpegBusiness.GetAutoCropRect(settings, true));
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
        
        private async Task GetMediaInfo(string previewFile, MediaEncoderSettings settings) {
            using (MediaInfoReader InfoReader = new MediaInfoReader()) {
                await InfoReader.LoadInfoAsync(previewFile);
                settings.SourceWidth = InfoReader.Width;
                settings.SourceHeight = InfoReader.Height;
                settings.SourceAspectRatio = InfoReader.PixelAspectRatio ?? 1;
                if (settings.SourceAspectRatio != 1) {
                    // Get aspect ratio from FFMPEG which is more accurate.
                    float? SAR = FfmpegBusiness.GetPixelAspectRatio(settings);
                    if (SAR.HasValue)
                        settings.SourceAspectRatio = SAR.Value;
                }
                // Fix last track of VCDs that is widescreen.
                if (settings.SourceHeight == 288 && settings.SourceWidth == 352 && settings.SourceAspectRatio == 1.485f)
                settings.SourceAspectRatio = 1.092f;
                settings.SourceFrameRate = InfoReader.FrameRate;
                if (settings.ConvertToAvi)
                    await InfoReader.LoadInfoAsync(Settings.NaturalGroundingFolder + settings.FileName);
                settings.SourceAudioFormat = InfoReader.AudioFormat;
                settings.SourceVideoFormat = InfoReader.VideoFormat;
                settings.SourceColorMatrix = InfoReader.Height < 720 ? ColorMatrix.Rec601 : ColorMatrix.Rec709;
                if (!settings.CanCopyAudio)
                    settings.EncodeFormat = VideoFormats.Mkv;
                settings.SourceAudioBitrate = InfoReader.AudioBitRate;
                settings.SourceBitDepth = InfoReader.BitDepth;
                settings.Position = (InfoReader.Length ?? 0) / 2;
                settings.CalculateSize();
            }
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
                    jobInfo.Settings.FileExtension);
            SafeMove(Settings.NaturalGroundingFolder + jobInfo.NewFileName, FinalFile);
        }

        public void DeleteJobFiles(MediaEncoderSettings settings) {
            if (settings.ConvertToAvi)
                File.Delete(settings.InputFile);
            File.Delete(settings.OutputFile);
            File.Delete(settings.AudioFileWav);
            File.Delete(settings.AudioFileAac);
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
            if (settings.ConvertToAvi)
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
                if (!File.Exists(PreviewSourceFile) && File.Exists(Settings.NaturalGroundingFolder + settings.FileName)) {
                    double? SourceFps = settings.SourceFrameRate; // Keep FPS in case it cannot be read from the file.
                    await PreparePreviewFile(settings, false);
                    if (!settings.SourceFrameRate.HasValue)
                        settings.SourceFrameRate = SourceFps;
                }
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
                    settings = LoadSettingsFile(item);
                }
                catch { }

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
                        } else if (File.Exists(settings.OutputFile) && new FileInfo(settings.OutputFile).Length > 0) {
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
            return Process.GetProcessesByName("ffmpeg").FirstOrDefault();
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

        public void SaveSettingsFile(MediaEncoderSettings settings, string fileName) {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (var writer = new StreamWriter(fileName)) {

                var serializer = new XmlSerializer(typeof(MediaEncoderSettings));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.NewLineHandling = NewLineHandling.Entitize;
                using (XmlWriter wr = XmlWriter.Create(writer, ws)) {
                    serializer.Serialize(wr, settings, ns);
                }
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

        /// <summary>
        /// The first time the media encoder window is shown, it will test whether OpenCL 1.2 is support to use the latest version 
        /// of KNLMeans. If not, an older version of the library will be used.
        /// </summary>
        public SupportedOpenClVersion GpuSupport {
            get {
                if (gpuSupportsOpenCL12 == SupportedOpenClVersion.NotTested) {
                    gpuSupportsOpenCL12 = FfmpegBusiness.TestSupportedOpenClVersion();
                }
                return gpuSupportsOpenCL12;
            }
        }

        public int ConvertAudioQualityToBitrate(int q) {
            return (int)Math.Round(400 * Math.Pow(q / 100.0, 1.28) + 8, 0);
            //400 * B22 ^ 1.28 + 8
        }

        public int ConvertAudioBitrateToQuality(int bitrate) {
            return (int)Math.Round(Math.Pow((bitrate - 8) / 400, 1 / 1.28) / 100.0, 0);
            //((C22 - 8) / 400) ^ (1 / 1.28)
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

    [Serializable()]
    public class Rect {
        public Rect() {
        }

        public Rect(int left, int top, int right, int bottom)
            : this() {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public bool HasValue {
            get {
                return Left != 0 || Top != 0 || Right != 0 || Bottom != 0;
            }
        }
    }
}
