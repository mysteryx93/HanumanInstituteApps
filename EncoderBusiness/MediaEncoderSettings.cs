using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using EmergenceGuardian.FFmpeg;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EmergenceGuardian.MediaEncoder {
    [Serializable()]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class MediaEncoderSettings {
        public string FilePath { get; set; }
        public string DisplayName { get; set; }
        public bool ConvertToAvi { get; set; }
        [DefaultValue(null)]
        public double? Position { get; set; }
        private int? sourceHeight;
        [DefaultValue(null)]
        public int? SourceHeight {
            get { return sourceHeight; }
            set {
                sourceHeight = value;
                CalculateSize(false);
            }
        }
        private int? sourceWidth;
        [DefaultValue(null)]
        public int? SourceWidth {
            get { return sourceWidth; }
            set {
                sourceWidth = value;
                CalculateSize(false);
            }
        }
        private float? sourceAspectRatio;
        public float? SourceAspectRatio {
            get { return sourceAspectRatio; }
            set {
                sourceAspectRatio = value;
                CalculateSize(false);
            }
        }
        [DefaultValue(null)]
        public double? SourceFrameRate { get; set; }
        [DefaultValue(null)]
        public string SourceAudioFormat { get; set; }
        [DefaultValue(null)]
        public int? SourceAudioBitrate { get; set; }
        [DefaultValue(null)]
        public string SourceVideoFormat { get; set; }
        [DefaultValue(null)]
        public int? SourceVideoBitrate { get; set; }
        public ColorMatrix SourceColorMatrix { get; set; }
        public ChromaPlacement SourceChromaPlacement { get; set; }
        public int? SourceBitDepth { get; set; }
        private int outputHeight;
        public int OutputHeight {
            get { return outputHeight; }
            set {
                outputHeight = value;
                CalculateSize(false);
            }
        }
        public bool Denoise { get; set; }
        public int DenoiseSharp { get; set; }
        public int DenoiseRn { get; set; }
        public bool DenoisePrescan { get; set; }
        public bool FixDoubleFrames { get; set; }
        public bool Dering { get; set; }
        public bool Deblock { get; set; }
        public bool Degrain { get; set; }
        public int DegrainStrength { get; set; }
        public bool DegrainSharp { get; set; }
        public DegrainPrefilters DegrainPrefilter { get; set; }
        public bool Deshaker { get; set; }
        public bool IncreaseFrameRate { get; set; }
        public FrameRateModeEnum IncreaseFrameRateValue { get; set; }
        public FrameRatePresetEnum IncreaseFrameRatePreset { get; set; }
        public bool IncreaseFrameRatePrefilter { get; set; }
        public UpscaleMethods UpscaleMethod { get; set; }
        public DownscaleMethods DownscaleMethod { get; set; }
        public int SuperXbrStrength { get; set; }
        public int SuperXbrSharpness { get; set; }
        public bool SuperRes { get; set; }
        public bool SuperRes3Passes { get; set; }
        public int SuperResStrength { get; set; }
        public int SuperResSoftness { get; set; }
        public float SSimStrength { get; set; }
        public bool SSimSoft { get; set; }
        private bool crop;
        public bool Crop {
            get { return crop; }
            set {
                crop = value;
                CalculateSize(false);
            }
        }
        private int cropLeft;
        public int CropLeft {
            get { return cropLeft; }
            set {
                cropLeft = value;
                CalculateSize(false);
            }
        }
        private int cropTop;
        public int CropTop {
            get { return cropTop; }
            set {
                cropTop = value;
                CalculateSize(false);
            }
        }
        private int cropRight;
        public int CropRight {
            get { return cropRight; }
            set {
                cropRight = value;
                CalculateSize(false);
            }
        }
        private int cropBottom;
        public int CropBottom {
            get { return cropBottom; }
            set {
                cropBottom = value;
                CalculateSize(false);
            }
        }
        public bool Trim { get; set; }
        [DefaultValue(null)]
        public int? TrimStart { get; set; }
        [DefaultValue(null)]
        public int? TrimEnd { get; set; }
        public bool ChangeSpeed { get; set; }
        public float ChangeSpeedValue { get; set; }

        private VideoAction videoAction;
        public VideoAction VideoAction {
            get { return videoAction; }
            set {
                videoAction = value;
                if (videoAction == VideoAction.Copy) {
                    Trim = false;
                    ChangeSpeed = false;
                }
                SetContainer();
            }
        }
        public int EncodeQuality { get; set; }
        public EncodePresets EncodePreset { get; set; }
        [XmlIgnore()]
        public int EncodePresetInt {
            get { return (int)EncodePreset; }
            set { EncodePreset = (EncodePresets)value; }
        }
        public string Container { get; set; }
        private AudioActions audioAction;
        public AudioActions AudioAction {
            get { return audioAction; }
            set {
                audioAction = value;
                if (AudioAction == AudioActions.Copy) {
                    Trim = false;
                    ChangeSpeed = false;
                }
                SetContainer();
            }
        }
        public int AudioQuality { get; set; }
        [DefaultValue(null)]
        public float? AudioGain { get; set; }
        public bool ChangeAudioPitch { get; set; }
        public MediaEncoderDeshakerSettings DeshakerSettings { get; set; }

        private bool autoCalculateSize;
        [XmlIgnore()]
        public bool AutoCalculateSize {
            get { return autoCalculateSize; }
            set {
                if (value && autoCalculateSize != value) {
                    autoCalculateSize = value;
                    CalculateSize(false);
                } else
                    autoCalculateSize = value;
            }
        }
        [XmlIgnore()]
        public int? OutputWidth { get; set; }
        [XmlIgnore()]
        public Rect CropSource { get; set; }
        [XmlIgnore()]
        public Rect CropAfter { get; set; }
        [XmlIgnore()]
        public int FrameDouble { get; set; }
        [XmlIgnore()]
        public int ScaleFactor { get; set; }
        [XmlIgnore()]
        public bool IsUpscaling { get; set; }
        [XmlIgnore()]
        public bool IsDownscaling { get; set; }
        [XmlIgnore()]
        public bool IsSameSize { get; set; }
        [XmlIgnore()]
        public long ResumePos { get; set; }

        [NonSerialized()]
        private List<FFmpegProcess> processes = new List<FFmpegProcess>();
        [XmlIgnore()]
        public List<FFmpegProcess> Processes {
            get { return processes; }
            set { processes = value; }
        }
        [XmlIgnore()]
        public CompletionStatus CompletionStatus { get; set; }

        public string CustomScript { get; set; }
        public int JobIndex { get; set; }
        [XmlIgnore()]
        public int ParallelProcessing {
            get {
                return RunInParallel ? Environment.ProcessorCount / 2 : 0;
            }
        }
        public int Threads {
            get {
                return RunInParallel ? 1 : Environment.ProcessorCount;
            }
        }
        public bool RunInParallel {
            get {
                return Denoise || (IncreaseFrameRate && IncreaseFrameRatePreset >= FrameRatePresetEnum.Normal);
            }
        }

        public MediaEncoderSettings() {
            JobIndex = -1;
            SourceColorMatrix = ColorMatrix.Rec601;
            SourceChromaPlacement = ChromaPlacement.MPEG2;
            OutputHeight = 768;
            Denoise = true;
            DenoiseSharp = 13;
            DenoiseRn = 10;
            Degrain = false;
            DegrainStrength = 10;
            DegrainSharp = true;
            IncreaseFrameRate = true;
            IncreaseFrameRateValue = FrameRateModeEnum.fps60;
            IncreaseFrameRatePreset = FrameRatePresetEnum.Slower;
            UpscaleMethod = UpscaleMethods.SuperXbr;
            DownscaleMethod = DownscaleMethods.Bicubic;
            SuperXbrStrength = 27;
            SuperXbrSharpness = 13;
            SuperRes = true;
            SuperRes3Passes = true;
            SuperResStrength = 100;
            SuperResSoftness = 0;
            SSimStrength = 80;
            SSimSoft = false;
            TrimStart = 0;
            ChangeSpeedValue = 100;
            VideoAction = VideoAction.x265;
            EncodeQuality = 22;
            EncodePreset = EncodePresets.medium;
            AudioQuality = 256;
            ChangeAudioPitch = false;
            DeshakerSettings = new MediaEncoderDeshakerSettings();
            DeshakerSettings.Segments = new ObservableCollection<MediaEncoderDeshakerSegmentSettings>();
            DeshakerSettings.Segments.Add(new MediaEncoderDeshakerSegmentSettings());
        }

        /// <summary>
        /// Calculates FrameDouble, CropSource, CropAfter and OutputWidth values.
        /// </summary>
        public void CalculateSize() {
            CalculateSize(true);
        }

        /// <summary>
        /// Calculates FrameDouble, CropSource, CropAfter and OutputWidth values.
        /// </summary>
        /// <param name="force">When false, this call is ignored when AutoCalculateSize is false.</param>
        private void CalculateSize(bool force) {
            if (!force && !AutoCalculateSize)
                return;

            if (!SourceWidth.HasValue || !SourceHeight.HasValue || OutputHeight > 10000) {
                OutputWidth = null;
                return;
            }

            // Calculate CropSource and CropAfter values.
            if (Crop) {
                // CropSource must be a factor of 4.
                CropSource = new Rect(
                    CropLeft / 4 * 4,
                    CropTop / 4 * 4,
                    CropRight / 4 * 4,
                    CropBottom / 4 * 4);
            } else
                CropSource = new Rect();

            FrameDouble = 0;
            ScaleFactor = 1;
            int FrameDoubleX = 0, FrameDoubleY = 0;
            int ScaleFactorX = 1, ScaleFactorY = 1;
            CalcOutputWidth(1);
            // Calculate scale factor based on height
            while ((SourceHeight - CropSource.Top - CropSource.Bottom) * ScaleFactorY - CropAfter.Top - CropAfter.Bottom < OutputHeight) {
                FrameDoubleY += 1;
                ScaleFactorY *= 2;
            }
            if (SourceAspectRatio != 1) {
                // Calculate scale factor based on width
                while ((int)Math.Round(((double)(SourceWidth - CropSource.Left - CropSource.Right) * ScaleFactorX - CropAfter.Left - CropAfter.Right) / 4) * 4 < OutputWidth) {
                    FrameDoubleX += 1;
                    ScaleFactorX *= 2;
                }
            }
            // Take highest ratio
            FrameDouble = FrameDoubleX > FrameDoubleY ? FrameDoubleX : FrameDoubleY;
            ScaleFactor = ScaleFactorX > ScaleFactorY ? ScaleFactorX : ScaleFactorY;

            CalcOutputWidth(ScaleFactor);

            IsUpscaling = FrameDouble > 0;
            IsDownscaling = !IsUpscaling && OutputHeight < SourceHeight - (Crop ? CropTop + CropBottom : 0);
            IsSameSize = !IsUpscaling && !IsDownscaling;
            DownscaleMethod = IsDownscaling ? DownscaleMethods.SSim : DownscaleMethods.Bicubic;
        }

        // This must be calculated twice; once to get the Width, and again to calculate accurate cropping values.
        public void CalcOutputWidth(int ScaleFactor) {
            if (Crop) {
                CropAfter = new Rect(
                    (CropLeft - CropSource.Left) * ScaleFactor,
                    (CropTop - CropSource.Top) * ScaleFactor,
                    (CropRight - CropSource.Right) * ScaleFactor,
                    (CropBottom - CropSource.Bottom) * ScaleFactor);
                // Make sure Height is Mod 2
                if (ScaleFactor == 1) {
                    if (CropAfter.Top + CropAfter.Bottom % 2 == 1)
                        CropAfter.Bottom++;
                    if (CropAfter.Left + CropAfter.Right % 2 == 1)
                        CropAfter.Right++;
                }
            } else
                CropAfter = new Rect();
            int CropHeight = ((SourceHeight.Value - CropSource.Top - CropSource.Bottom) * ScaleFactor) - CropAfter.Top - CropAfter.Bottom;
            int CropWidth = ((SourceWidth.Value - CropSource.Left - CropSource.Right) * ScaleFactor) - CropAfter.Left - CropAfter.Right;
            // Make width divisible by 4 without distorting pixels
            float TotalWidth = (float)CropWidth * SourceAspectRatio.Value / CropHeight * OutputHeight;
            OutputWidth = (int)Math.Round(TotalWidth / 4) * 4;
            if (TotalWidth >= OutputWidth.Value) {
                float WidthAdjust = TotalWidth - OutputWidth.Value;
                int WidthAdjustInt = (int)Math.Round(WidthAdjust / 2);
                CropAfter.Left += WidthAdjustInt;
                CropAfter.Right += WidthAdjustInt;
            } else {
                float HeightAdjust = (OutputWidth.Value - TotalWidth) / SourceAspectRatio.Value;
                int HeightAdjustInt = (int)Math.Round(HeightAdjust / 2);
                CropAfter.Top += HeightAdjustInt;
                CropAfter.Bottom += HeightAdjustInt;
            }
        }

        public void SetContainer() {
            if (videoAction == VideoAction.Avi || videoAction == VideoAction.AviUtVideo || (videoAction == VideoAction.Copy && IsVideoAvi))
                Container = "avi";
            else if (videoAction == VideoAction.x264 || videoAction == VideoAction.x264_10bit || videoAction == VideoAction.x265 || videoAction == VideoAction.xvid || (videoAction == VideoAction.Copy && IsSourceVideoMp4)) {
                if (audioAction == AudioActions.Aac || audioAction == AudioActions.Mp3 || audioAction == AudioActions.Discard || (audioAction == AudioActions.Copy && IsSourceAudioMp4))
                    Container = "mp4";
                else
                    Container = "mkv";
            } else if (videoAction == VideoAction.Discard) {
                if (audioAction == AudioActions.Aac)
                    Container = "m4a";
                else if (audioAction == AudioActions.Flac)
                    Container = "flac";
                else if (audioAction == AudioActions.Opus)
                    Container = "opus";
                else if (audioAction == AudioActions.Wav)
                    Container = "wav";
                else if (audioAction == AudioActions.Mp3)
                    Container = "mp3";
                else
                    Container = "";
            } else
                Container = "mkv";
        }

        public bool HasVideoOptions {
            get {
                return videoAction == VideoAction.x264 || videoAction == VideoAction.x264_10bit || videoAction == VideoAction.x265;
            }
        }

        public bool HasAudioOptions {
            get {
                return audioAction == AudioActions.Aac || audioAction == AudioActions.Opus || audioAction == AudioActions.Flac || audioAction == AudioActions.Wav || audioAction == AudioActions.Mp3;
            }
        }

        public bool IsAudioCompress {
            get {
                return AudioAction == AudioActions.Aac || AudioAction == AudioActions.Opus || audioAction == AudioActions.Mp3;
            }
        }

        public bool IsVideoAvi {
            get {
                return string.IsNullOrEmpty(SourceVideoFormat) || new string[] { "huffyuv", "utvideo" }.Contains(SourceVideoFormat.ToLower());
            }
        }

        public bool IsSourceVideoMp4 {
            get {
                return string.IsNullOrEmpty(SourceVideoFormat) || new string[] { "mpeg4", "h264", "hevc", "h263p", "h263i" }.Contains(SourceVideoFormat.ToLower());
            }
        }

        public bool IsSourceAudioMp4 {
            get {
                return string.IsNullOrEmpty(SourceAudioFormat) || new string[] { "aac", "ac3", "mp3" }.Contains(SourceAudioFormat.ToLower());
            }
        }

        public bool CanCopyAudio {
            get {
                return !Trim && !ChangeSpeed;
            }
        }

        public bool CanAlterAudio {
            get {
                return audioAction != AudioActions.Copy && VideoAction != VideoAction.Copy;
            }
        }

        public bool HasFilePath {
            get { return !string.IsNullOrEmpty(FilePath); }
        }

        public string ScriptFile {
            get { return PathManager.GetScriptFile(JobIndex, -1); }
        }

        public string SettingsFile {
            get { return PathManager.GetSettingsFile(JobIndex); }
        }

        public string InputFile {
            get {
                if (ConvertToAvi)
                    return PathManager.GetInputFile(JobIndex);
                else
                    return FilePath;
            }
        }

        public string OutputFile {
            get { return PathManager.GetOutputFile(JobIndex, ResumePos, Container); }
        }

        public string OutputScriptFile {
            get { return PathManager.GetScriptFile(JobIndex, ResumePos); }
        }

        public string AudioFile {
            get { return PathManager.GetAudioFile(JobIndex, AudioAction); }
        }

        public string FinalFile {
            get { return PathManager.GetFinalFile(JobIndex, Container); }
        }

        public string TempFile {
            get { return PathManager.GetTempFile(JobIndex); }
        }

        public string DeshakerScript {
            get { return PathManager.GetDeshakerScript(JobIndex); }
        }

        public string DeshakerTempOut {
            get { return PathManager.GetDeshakerTempOut(JobIndex); }
        }

        public string DeshakerTempLog {
            get { return PathManager.GetDeshakerTempLog(JobIndex); }
        }

        public string DeshakerLog {
            get { return PathManager.GetDeshakerLog(JobIndex); }
        }

        public MediaEncoderSettings Clone() {
            return (MediaEncoderSettings)this.DeepClone();
        }


        public void Save(string filePath) {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            // Don't serialize DeshakerSesttings unless Deshaker is enabled, but preserve settings within class.
            MediaEncoderDeshakerSettings S = DeshakerSettings;
            if (!Deshaker)
                DeshakerSettings = null;

            using (var writer = new StreamWriter(filePath)) {
                var serializer = new XmlSerializer(typeof(MediaEncoderSettings));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                XmlWriterSettings ws = new XmlWriterSettings();
                // ws.NewLineHandling = NewLineHandling.Entitize;
                using (XmlWriter wr = XmlWriter.Create(writer, ws)) {
                    serializer.Serialize(wr, this, ns);
                }
                writer.Flush();
            }

            if (!Deshaker)
                DeshakerSettings = S;
        }

        public static MediaEncoderSettings Load(string filePath) {
            using (var stream = File.OpenRead(filePath)) {
                var serializer = new XmlSerializer(typeof(MediaEncoderSettings));
                MediaEncoderSettings Result = serializer.Deserialize(stream) as MediaEncoderSettings;
                if (Result.DeshakerSettings == null)
                    Result.DeshakerSettings = new MediaEncoderDeshakerSettings();
                // Serialization changes new lines; restore them.
                if (Result.CustomScript != null)
                    Result.CustomScript = Result.CustomScript.Replace("\n", "\r\n");
                return Result;
            }
        }

        public string Display {
            get {
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }

        /// <summary>
        /// Cancels any running processes.
        /// </summary>
        public void Cancel() {
            CompletionStatus = CompletionStatus.Cancelled;
            foreach (FFmpegProcess worker in Processes) {
                worker.Cancel();
            }
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

    public enum ColorMatrix {
        Rec601,
        Rec709,
        Pc601,
        Pc709
    }

    public enum ChromaPlacement {
        MPEG1,
        MPEG2,
        DV
    }

    public enum DegrainPrefilters {
        SD,
        HD,
        KNLMeans
    }

    public enum UpscaleMethods {
        SuperXbr,
        NNedi3
    }

    public enum DownscaleMethods {
        SSim,
        Bicubic
    }

    public enum VideoAction {
        Copy,
        Discard,
        x264,
        x264_10bit,
        x265,
        xvid,
        Avi,
        AviUtVideo
    }
    
    public enum AudioActions {
        Copy,
        Discard,
        Wav,
        Flac,
        Aac,
        Opus,
        Mp3
    }

    public enum EncodePresets {
        faster = 0,
        fast = 1,
        medium = 2,
        slow = 3,
        slower = 4,
        veryslow = 5,
        placebo = 6
    }

    public enum FrameRateModeEnum {
        Double = 0,
        fps30 = 1,
        fps60 = 2,
        fps120 = 3
    }

    public enum FrameRatePresetEnum {
        Faster = 0,
        Fast = 1,
        Normal = 2,
        Slow = 3,
        Slower = 4,
        Slowest = 5,
        Anime = 6
    }

}
