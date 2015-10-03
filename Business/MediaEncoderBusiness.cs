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
            string Script = settings.CustomScript;
            if (string.IsNullOrEmpty(Script))
                Script = GenerateVideoScript(settings, GetPreviewSourceFile(settings), preview, multiThreaded);
            else if (preview)
                Script = ConvertScriptForPreview(Script);
            WriteFile(Script, PreviewScriptFile);
            SaveSettingsFile(settings, PreviewSettingsFile);
        }

        public void GenerateCustomScript(MediaEncoderSettings settings) {
            settings.CustomScript = GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true);
        }

        public bool CustomScriptHasChanges(MediaEncoderSettings settings) {
            return settings.CustomScript != GenerateVideoScript(settings, GetPreviewSourceFile(settings), false, true);
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
            string Script = settings.CustomScript;
            if (string.IsNullOrEmpty(Script))
                Script = GenerateVideoScript(settings, settings.InputFile, false, true);
            else
                Script = Script.Replace(GetAsciiPath(PreviewSourceFile), GetAsciiPath(settings.InputFile));
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
            FfmpegBusiness.ConvertToH264(settings.ScriptFile, settings.OutputFile, settings.EncodeQuality, settings.EncodePreset);
            Result = FinalizeEncoding(settings, StartTime);
            return Result;
        }

        private EncodingCompletedEventArgs FinalizeEncoding(MediaEncoderSettings settings, DateTime? startTime) {
            EncodingCompletedEventArgs Result = null;
            if (File.Exists(settings.OutputFile)) {
                // Muxe video with audio.
                string FinalFile = GetNextAvailableFileName(settings.FinalFile);
                if (settings.AudioAction == AudioActions.Copy)
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

        public string GenerateVideoScript(MediaEncoderSettings settings, string inputFile, bool preview, bool multiThreaded) {
            int CPU = Environment.ProcessorCount;
            //if (settings.DoubleEEDI3)
            //    CPU = 1 + CPU / 4; // Limit threads when using EEDI3 which already uses most cores.
            bool AviSynthPlus = false;
            // multiThreaded = true;

            // Calculate CropSource and CropAfter values.
            Rect CropSource = new Rect();
            Rect CropAfter = new Rect();
            if (settings.Crop) {
                // CropSource must be a factor of two.
                CropSource = new Rect(
                    settings.CropLeft / 2 * 2,
                    settings.CropTop / 2 * 2,
                    settings.CropRight / 2 * 2,
                    settings.CropBottom / 2 * 2);
            }
            int ScaleFactor = (settings.DoubleNNEDI3Before ? 2 : 1) * (settings.DoubleEEDI3 ? 2 : 1) * (settings.DoubleNNEDI3 ? 2 : 1);
            int ExtraDouble = 0;
            // If cropping makes the output smaller than the resize dimension, keep doubling to enlarge.
            if (settings.Resize) {
                while ((settings.SourceHeight - CropSource.Top - CropSource.Bottom) * ScaleFactor < settings.ResizeHeight) {
                    ExtraDouble += 2;
                    ScaleFactor *= 2;
                }
            }
            if (settings.Crop) {
                CropAfter = new Rect(
                    (settings.CropLeft - CropSource.Left) * ScaleFactor,
                    (settings.CropTop - CropSource.Top) * ScaleFactor,
                    (settings.CropRight - CropSource.Right) * ScaleFactor,
                    (settings.CropBottom - CropSource.Bottom) * ScaleFactor);
            }
            if (!settings.Resize) // If pixels are not square, we must always resize to preserve aspect ratio.
                settings.ResizeHeight = settings.SourceHeight.Value * ScaleFactor;
            int CropHeight = ((settings.SourceHeight.Value - CropSource.Top - CropSource.Bottom) * ScaleFactor);
            int CropWidth = ((settings.SourceWidth.Value - CropSource.Left - CropSource.Right) * ScaleFactor);
            if (CropAfter.HasValue) {
                CropHeight = CropHeight - CropAfter.Top - CropAfter.Bottom;
                CropWidth = CropWidth - CropAfter.Left - CropAfter.Right;
            }
            int FinalHeight = settings.Resize ? settings.ResizeHeight : CropHeight;
            int FinalWidth = (int)Math.Round((double)CropWidth * settings.SourceAspectRatio / CropHeight * FinalHeight / 4) * 4;

            // Calculate encoding and final frame rates.
            double ChangeSpeedValue = settings.ChangeSpeed ? (double)settings.ChangeSpeedValue / 100 : 1;
            double FrameRateBefore = settings.SourceFrameRate.Value * ChangeSpeedValue;

            // Generate video script.
            StringBuilder Script = new StringBuilder();
            Script.AppendLine(); // After clean-up, this adds a line after LoadPlugin commands.
            if (multiThreaded) {
                // Script.AppendLine("Setmemorymax(1500)");
                if (AviSynthPlus) {
                    Script.AppendLine(@"SetFilterMTMode(""DEFAULT_MT_MODE"",2)");
                    Script.AppendLine(@"SetFilterMTMode(""AviSource"",3)");
                    if (settings.Denoise1)
                        Script.AppendLine(@"SetFilterMTMode(""KNLMeansCL"",5)");
                } else {
                    Script.AppendFormat("SetMTMode(3,{0})", CPU).AppendLine();
                }
            }
            Script.AppendFormat(@"PluginPath=""{0}""", GetAsciiPath(Settings.AviSynthPluginsPath)).AppendLine();
            if (settings.ConvertToAvi)
                Script.AppendFormat(@"AviSource(""{0}"", audio=false, pixel_type=""YV12"")", GetAsciiPath(inputFile)).AppendLine(); //.ConvertToYV12()
            else
                Script.AppendFormat(@"DirectShowSource(""{0}"", audio=false, pixel_type=""YV12""{1})", GetAsciiPath(inputFile), settings.SourceFrameRate.HasValue ? ", fps=" + settings.SourceFrameRate : string.Empty).AppendLine(); //.ConvertToYV12()
            if (multiThreaded && !AviSynthPlus)
                Script.AppendLine("SetMTMode(2)");
            if (settings.SourceColorMatrix == ColorMatrix.Rec601 && !preview) {
                LoadPluginDll(Script, "ColorMatrix.dll");
                Script.AppendLine(@"ColorMatrix(mode=""Rec.601->Rec.709"")");
            }
            if (FrameRateBefore != settings.SourceFrameRate)
                Script.AppendFormat(CultureInfo.InvariantCulture, "AssumeFPS({0})", FrameRateBefore).AppendLine();
            if (settings.Trim) {
                Script.AppendFormat("Trim({0}, {1})",
                    settings.TrimStart.HasValue ? (int)(settings.TrimStart.Value * settings.SourceFrameRate.Value) : 0,
                    settings.TrimEnd.HasValue && !preview ? (int)(settings.TrimEnd.Value * settings.SourceFrameRate.Value) : 0).AppendLine();
            }
            if (CropSource.HasValue) {
                Script.AppendFormat("Crop({0}, {1}, -{2}, -{3})", CropSource.Left, CropSource.Top, CropSource.Right, CropSource.Bottom).AppendLine();
            }
            if (settings.Denoise1) {
                LoadPluginDll(Script, "KNLMeansCL.dll");
                if (multiThreaded && !AviSynthPlus)
                    Script.AppendLine("SetMTMode(5)");
                Script.AppendFormat(CultureInfo.InvariantCulture, @"KNLMeansCL(D=2, A=1, h={0}, device_type=""GPU"")", ((double)settings.Denoise1Strength / 10).ToString(CultureInfo.InvariantCulture)).AppendLine();
                if (multiThreaded && !AviSynthPlus)
                    Script.AppendLine("SetMTMode(2)");
            }
            if (settings.Denoise2) {
                LoadPluginDll(Script, "FFT3DFilter.dll");
                Script.AppendFormat(CultureInfo.InvariantCulture, @"fft3dfilter(sigma={0}, bt=5, bw=48, bh=48, ow=24, oh=24, sharpen={1})",
                    ((double)settings.Denoise2Strength / 10).ToString(CultureInfo.InvariantCulture),
                    (double)settings.Denoise2Sharpen / 100).AppendLine();
            }
            //} else if (settings.SharpenFinal)
            //    LoadPluginDll(Script, "FFT3DFilter.dll");
            if (settings.SuperRes || settings.SharpenFinal) {
                LoadPluginDll(Script, "Shader.dll");
                LoadPluginAvsi(Script, "SuperRes.avsi");
            }
            if (settings.DoubleNNEDI3 || settings.DoubleNNEDI3Before || ExtraDouble > 0) {
                LoadPluginDll(Script, "nnedi3.dll");
                LoadPluginDll(Script, "fturn.dll"); // The UI doesn't allow applying EEDI3 twice so FTurn is only necessary when NNEDI3 is used.
            }
            int DoubleCount = 0; // InterFrame can be applied in either of 2 locations based on the frame double order.
            if (settings.DoubleNNEDI3Before) {
                DoubleCount++;
                bool HasOtherDouble = settings.SuperRes && (settings.DoubleEEDI3 || (settings.DoubleNNEDI3 || ExtraDouble > 0));
                string DoubleScript = string.Format(@"nnedi3_rpow2(2, nns=4, {0}, Threads=2)", GetPixelShiftResize(HasOtherDouble, FinalWidth, FinalHeight, CropAfter));
                DoubleAndSharpen(Script, settings, HasOtherDouble, DoubleScript);
            }
            if (DoubleCount == 1)
                ApplyInterFrame(Script, settings, CPU);
            if (settings.DoubleEEDI3) {
                DoubleCount++;
                bool HasOtherDouble = settings.SuperRes && (settings.DoubleNNEDI3 || ExtraDouble > 0);
                LoadPluginDll(Script, "eedi3.dll");
                string DoubleScript = string.Format("eedi3_rpow2(2, {0}, Threads=2)", GetPixelShiftResize(HasOtherDouble, FinalWidth, FinalHeight, CropAfter));
                DoubleAndSharpen(Script, settings, HasOtherDouble, DoubleScript);
            }
            if (DoubleCount <= 1)
                ApplyInterFrame(Script, settings, CPU);
            if (settings.DoubleNNEDI3)
                ExtraDouble++;
            if (ExtraDouble > 0) {
                for (int i = 0; i < ExtraDouble; i++) {
                    DoubleCount++;
                    string DoubleScript = string.Format("nnedi3_rpow2(2, nns=4, {0}, Threads=2)", GetPixelShiftResize(false, FinalWidth, FinalHeight, CropAfter));
                    DoubleAndSharpen(Script, settings, i < ExtraDouble - 1, DoubleScript);
                }
            }
            //if (settings.Resize || settings.SourceAspectRatio != 1 || CropAfter.HasValue) {
            // Calculate width for resize.
            if (CropAfter.HasValue) {
                Script.AppendFormat("Spline36Resize({0}, {1}, {2}, {3}, -{4}, -{5})",
                    FinalWidth, FinalHeight, CropAfter.Left, CropAfter.Top, CropAfter.Right, CropAfter.Bottom).AppendLine();
            } else {
                Script.AppendFormat("Spline36Resize({0}, {1})", FinalWidth, FinalHeight).AppendLine();
            }
            //}
            if (preview && (settings.Crop))
                Script.AppendLine("AddBorders(2, 2, 2, 2, $FFFFFF)");
            if (preview)
                Script.AppendLine("ConvertToRGB32()");
            if (multiThreaded) {
                if (AviSynthPlus)
                    Script.AppendFormat("Prefetch({0})", CPU).AppendLine();
                else
                    Script.AppendLine("Distributor()");
            }

            return CleanupScript(Script.ToString());
        }

        public void ApplyInterFrame(StringBuilder Script, MediaEncoderSettings settings, int CPU) {
            if (settings.IncreaseFrameRate) {
                LoadPluginDll(Script, "svpflow1.dll");
                LoadPluginDll(Script, "svpflow2.dll");
                LoadPluginAvsi(Script, "InterFrame2.avsi");
                if (settings.IncreaseFrameRateValue == FrameRateModeEnum.Double)
                    Script.AppendFormat(@"InterFrame(Cores={0}, Tuning=""Smooth"", FrameDouble=true{1})", CPU, Settings.SavedFile.EnableMadVR ? ", GPU=true" : "").AppendLine();
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
                    Script.AppendFormat(@"InterFrame(Cores={0}, Tuning=""Smooth"", NewNum={1}, NewDen={2}{3})", CPU, NewNum, NewDen, Settings.SavedFile.EnableMadVR ? ", GPU=true" : "").AppendLine();
                }
            }
        }

        public string GetPixelShiftResize(bool hasOtherDouble, int width, int height, Rect cropAfter) {
            if (hasOtherDouble)
                return @"cshift=""Spline16Resize""";
            else {
                return string.Format(@"cshift=""Spline36Resize"", fwidth={0}, fheight={1}",
                    width + cropAfter.Left + cropAfter.Right,
                    height + cropAfter.Top + cropAfter.Bottom);
            }
        }

        public void DoubleAndSharpen(StringBuilder Script, MediaEncoderSettings settings, bool hasOtherDouble, string upscaleScript) {
            if ((hasOtherDouble && settings.SuperRes) || (!hasOtherDouble && settings.SharpenFinal)) {
                float Strength = hasOtherDouble ? settings.SuperResStrength : settings.SharpenFinalStrength;
                Script.AppendFormat(CultureInfo.InvariantCulture, @"SuperRes({0}, {1}, {2}, true, """"""{3}"""""", ""{4}"")", 1, Strength / 100, settings.SuperResSoftness / 100, upscaleScript, GetAsciiPath(Settings.AviSynthPluginsPath)).AppendLine();
                // Script.AppendFormat(CultureInfo.InvariantCulture, @"fft3dfilter(bt=-1, sharpen={0})", Strength / 100).AppendLine();
                // Script.AppendFormat(@"LSFmod(strength={0}, defaults=""slow"")", settings.SharpenAfterDoubleStrength).AppendLine();
                //Script.AppendFormat(@"SuperRes(passes={0}, strength={1}, softness={2}, upscalecommand=""""""{3}"""""")", 5, Strength / 100, .25, upscaleScript).AppendLine();
            } else
                Script.AppendLine(upscaleScript);
        }

        /// <summary>
        /// Moves all LoadPlugin and Import commands to the beginning to make the script more readable.
        /// </summary>
        /// <param name="script">The script to clean up.</param>
        /// <returns>An updated script.</returns>
        public string CleanupScript(string script) {
            string[] Lines = script.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToComment = new string[] { "PluginPath", "LoadPlugin", "Import" };
            var NewScriptLines = Lines.Where(l => CommandsToComment.Any(c => l.StartsWith(c))).Concat(Lines.Where(l => !CommandsToComment.Any(c => l.StartsWith(c))));
            return string.Join(Environment.NewLine, NewScriptLines);
        }

        /// <summary>
        /// Removes commands from the script to allow displaying it as a preview.
        /// </summary>
        /// <param name="script">The script to modify.</param>
        /// <returns>An updated script.</returns>
        public string ConvertScriptForPreview(string script) {
            string[] Lines = script.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToComment = new string[] { "ColorMatrix", "SetMTMode", "Prefetch", "Distributor" };
            for (int i = 0; i < Lines.Length; i++) {
                if (CommandsToComment.Any(c => Lines[i].StartsWith(c)))
                    Lines[i] = "# " + Lines[i];
            }
            return string.Join(Environment.NewLine, Lines) + Environment.NewLine + "ConvertToRGB32()";
        }

        public void LoadPluginDll(StringBuilder script, string fileName) {
            script.AppendFormat(@"LoadPlugin(PluginPath+""{0}"")", fileName).AppendLine();
        }

        public void LoadPluginAvsi(StringBuilder script, string fileName) {
            script.AppendFormat(@"Import(PluginPath+""{0}"")", fileName).AppendLine();
        }

        private void WriteFile(string content, string fileName) {
            if (!Directory.Exists(Settings.TempFilesPath))
                Directory.CreateDirectory(Settings.TempFilesPath);

            File.WriteAllText(fileName, content, Encoding.ASCII);
        }

        public async Task OpenPreview(MediaEncoderSettings settings, bool overwrite) {
            if (string.IsNullOrEmpty(settings.FileName))
                return;

            if (overwrite)
                File.Delete(PreviewSourceFile);

            bool IsCompleted = File.Exists(PreviewSourceFile) || !settings.ConvertToAvi ||
                await Task.Run(() => FfmpegBusiness.ConvertToAVI(Settings.NaturalGroundingFolder + settings.FileName, PreviewSourceFile, false));
            if (IsCompleted && settings.ConvertToAvi)
                await GetMediaInfo(PreviewSourceFile, settings);
            else {
                settings.OpenMethod = OpenMethods.DirectShowSource;
                await GetMediaInfo(Settings.NaturalGroundingFolder + settings.FileName, settings);
            }
        }

        private async Task GetMediaInfo(string previewFile, MediaEncoderSettings settings) {
            MediaInfoReader InfoReader = new MediaInfoReader();
            await InfoReader.LoadInfoAsync(previewFile);
            settings.SourceWidth = InfoReader.Width;
            settings.SourceHeight = InfoReader.Height;
            settings.SourceAspectRatio = InfoReader.PixelAspectRatio ?? 1;
            // Fix last track of VCDs that is widescreen.
            if (settings.SourceHeight == 288 && settings.SourceWidth == 352 && settings.SourceAspectRatio == 1.485f)
                settings.SourceAspectRatio = 1.092f;
            settings.SourceFrameRate = InfoReader.FrameRate;
            if (settings.ConvertToAvi)
                await InfoReader.LoadInfoAsync(Settings.NaturalGroundingFolder + settings.FileName);
            settings.SourceAudioFormat = InfoReader.AudioFormat;
            if (!settings.CanCopyAudio)
                settings.EncodeFormat = VideoFormats.Mkv;
            settings.SourceAudioBitrate = InfoReader.AudioBitRate;
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
                    jobInfo.Settings.FileExtension);
            SafeMove(Settings.NaturalGroundingFolder + jobInfo.NewFileName, FinalFile);
        }

        public void DeleteJobFiles(MediaEncoderSettings settings) {
            if (settings.ConvertToAvi)
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
                    await OpenPreview(settings, false);
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
                }
                catch {
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

    public struct Rect {
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
