using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;

namespace Business {
    [PropertyChanged.ImplementPropertyChanged]
    [Serializable()]
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
        private float sourceAspectRatio;
        public float SourceAspectRatio {
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
        public int DenoiseStrength { get; set; }
        public int DenoiseD { get; set; }
        public int DenoiseA { get; set; }
        public bool Dering { get; set; }
        public bool Degrain { get; set; }
        public int DegrainStrength { get; set; }
        public bool DegrainSharp { get; set; }
        public DegrainPrefilters DegrainPrefilter { get; set; }
        public bool Deshaker { get; set; }
        public bool IncreaseFrameRate { get; set; }
        public FrameRateModeEnum IncreaseFrameRateValue { get; set; }
        public bool IncreaseFrameRateSmooth { get; set; }
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

        public VideoCodecs VideoCodec { get; set; }
        public float EncodeQuality { get; set; }
        public EncodePresets EncodePreset { get; set; }
        public VideoFormats EncodeFormat { get; set; }
        private AudioActions audioAction;
        public AudioActions AudioAction {
            get { return audioAction; }
            set {
                audioAction = value;
                if ((audioAction == AudioActions.Copy && !IsAudioMp4) || audioAction == AudioActions.EncodeOpus)
                    EncodeFormat = VideoFormats.Mkv;
                else if (audioAction == AudioActions.Ignore || audioAction == AudioActions.EncodeAac)
                    EncodeFormat = VideoFormats.Mp4;
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

        public string CustomScript { get; set; }
        public int JobIndex { get; set; }

        public MediaEncoderSettings() {
            JobIndex = -1;
            SourceAspectRatio = 1;
            SourceColorMatrix = ColorMatrix.Rec601;
            SourceChromaPlacement = ChromaPlacement.MPEG2;
            OutputHeight = 768;
            Denoise = true;
            DenoiseStrength = 10;
            DenoiseD = 2;
            DenoiseA = 2;
            Degrain = true;
            DegrainStrength = 10;
            DegrainSharp = true;
            IncreaseFrameRate = true;
            IncreaseFrameRateValue = FrameRateModeEnum.fps60;
            IncreaseFrameRateSmooth = true;
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
            VideoCodec = VideoCodecs.x265;
            EncodeQuality = 22;
            EncodePreset = EncodePresets.medium;
            AudioQuality = 256;
            ChangeAudioPitch = false;
            DeshakerSettings = new MediaEncoderDeshakerSettings();
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
            float TotalWidth = (float)CropWidth * SourceAspectRatio / CropHeight * OutputHeight;
            OutputWidth = (int)Math.Round(TotalWidth / 4) * 4;
            if (TotalWidth >= OutputWidth.Value) {
                float WidthAdjust = TotalWidth - OutputWidth.Value;
                int WidthAdjustInt = (int)Math.Round(WidthAdjust / 2);
                CropAfter.Left += WidthAdjustInt;
                CropAfter.Right += WidthAdjustInt;
            } else {
                float HeightAdjust = (OutputWidth.Value - TotalWidth) / SourceAspectRatio;
                int HeightAdjustInt = (int)Math.Round(HeightAdjust / 2);
                CropAfter.Top += HeightAdjustInt;
                CropAfter.Bottom += HeightAdjustInt;
            }
        }

        public bool IsAudioEncode {
            get {
                return AudioAction == AudioActions.EncodeAac || AudioAction == AudioActions.EncodeOpus;
            }
        }

        public bool CanEncodeMp4 {
            get {
                return IsAudioMp4 || AudioAction == AudioActions.Ignore || audioAction == AudioActions.EncodeAac;
            }
        }

        public bool IsAudioMp4 {
            get {
                return string.IsNullOrEmpty(SourceAudioFormat) || new string[] { "AAC", "AC3", "MP3" }.Contains(SourceAudioFormat);
            }
        }

        public bool CanCopyAudio {
            get {
                return (IsAudioMp4 || EncodeFormat == VideoFormats.Mkv) && !Trim && !ChangeSpeed;
            }
        }

        public bool CanAlterAudio {
            get {
                return audioAction != AudioActions.Copy;
            }
        }

        public string FileExtension {
            get {
                return EncodeFormat == VideoFormats.Mp4 ? "mp4" : "mkv";
            }
        }

        public bool HasFilePath {
            get { return !string.IsNullOrEmpty(FilePath); }
        }

        public string ScriptFile {
            get {
                if (JobIndex >= 0)
                    return Settings.TempFilesPath + string.Format("Job{0}_Script.avs", JobIndex);
                else
                    return MediaEncoderBusiness.PreviewScriptFile;
            }
        }

        public string SettingsFile {
            get {
                if (JobIndex >= 0)
                    return Settings.TempFilesPath + string.Format("Job{0}_Settings.xml", JobIndex);
                else
                    return MediaEncoderBusiness.PreviewSettingsFile;
            }
        }

        public string InputFile {
            get {
                if (ConvertToAvi) {
                    if (JobIndex >= 0)
                        return Settings.TempFilesPath + string.Format("Job{0}_Input.avi", JobIndex);
                    else
                        return MediaEncoderBusiness.PreviewSourceFile;
                } else
                    return FilePath;
            }
        }

        public string OutputFile {
            get {
                if (VideoCodec == VideoCodecs.Avi)
                    return Settings.TempFilesPath + string.Format("Job{0}_Output.avi", JobIndex);
                else
                    return Settings.TempFilesPath + string.Format("Job{0}_Output.mkv", JobIndex);
            }
        }

        public string AudioFileWav {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.wav", JobIndex); }
        }

        public string AudioFileAac {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.aac", JobIndex); }
        }

        public string AudioFileOpus {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.opus", JobIndex); }
        }

        public string FinalFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Final.{1}", JobIndex, EncodeFormat == VideoFormats.Mp4 ? "mp4" : "mkv"); }
        }

        public string TempFile {
            get {
                if (JobIndex >= 0)
                    return Settings.TempFilesPath + string.Format("Job{0}_Temp", JobIndex);
                else
                    return Settings.TempFilesPath + "Preview_Temp";
            }
        }

        public string DeshakerScript {
            get {
                if (JobIndex >= 0)
                    return Settings.TempFilesPath + string.Format("Job{0}_Deshaker.avs", JobIndex);
                else
                    return Settings.TempFilesPath + "Preview_Deshaker.avs";
            }
        }

        public string DeshakerLog {
            get {
                if (JobIndex >= 0)
                    return Settings.TempFilesPath + string.Format("Job{0}_Deshaker.log", JobIndex);
                else
                    return Settings.TempFilesPath + "Preview_Deshaker.log";
            }
        }

        public MediaEncoderSettings Clone() {
            return (MediaEncoderSettings)DataHelper.DeepClone(this);
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

    public enum VideoCodecs {
        Copy,
        x264,
        x265,
        Avi
    }

    public enum VideoFormats {
        Mp4,
        Mkv
    }

    public enum AudioActions {
        Copy,
        Ignore,
        EncodeAac,
        EncodeOpus
    }

    public enum EncodePresets {
        faster,
        fast,
        medium,
        slow,
        slower,
        veryslow,
        placebo
    }
}
