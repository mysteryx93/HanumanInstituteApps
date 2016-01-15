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

namespace Business {
    [PropertyChanged.ImplementPropertyChanged]
    [Serializable()]
    public class MediaEncoderSettings : ICloneable {
        public string FileName { get; set; }
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
        public int? SourceBitDepth { get; set; }
        private int outputHeight;
        public int OutputHeight {
            get { return outputHeight; }
            set {
                outputHeight = value;
                CalculateSize(false);
            }
        }
        public bool Denoise1 { get; set; }
        public int Denoise1Strength { get; set; }
        public bool Denoise1Compatibility { get; set; }
        public bool Denoise2 { get; set; }
        public int Denoise2Strength { get; set; }
        public int Denoise2Sharpen { get; set; }
        public bool IncreaseFrameRate { get; set; }
        public FrameRateModeEnum IncreaseFrameRateValue { get; set; }
        public bool IncreaseFrameRateSmooth { get; set; }
        public UpscaleMethods UpscaleMethod { get; set; }
        public int SuperXbrStrength { get; set; }
        public int SuperXbrSharpness { get; set; }
        public bool SuperRes { get; set; }
        public bool SuperResDoublePass { get; set; }
        public int SuperResStrength { get; set; }
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
                if (audioAction == AudioActions.Copy) {
                    Trim = false;
                    ChangeSpeed = false;
                    if (!IsAudioMp4)
                        EncodeFormat = VideoFormats.Mkv;
                }
            }
        }
        public int AudioQuality { get; set; }
        [DefaultValue(null)]
        public float? AudioGain { get; set; }
        public bool ChangeAudioPitch { get; set; }

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

        public string CustomScript { get; set; }
        public int JobIndex { get; set; }

        public MediaEncoderSettings() {
            JobIndex = -1;
            SourceAspectRatio = 1;
            SourceColorMatrix = ColorMatrix.Rec601;
            OutputHeight = 768;
            Denoise1 = true;
            Denoise1Strength = 21;
            Denoise2Strength = 30;
            Denoise2Sharpen = 10;
            IncreaseFrameRate = true;
            IncreaseFrameRateValue = FrameRateModeEnum.fps60;
            IncreaseFrameRateSmooth = true;
            UpscaleMethod = UpscaleMethods.SuperXbr;
            SuperXbrStrength = 20;
            SuperXbrSharpness = 12;
            SuperRes = true;
            SuperResDoublePass = true;
            SuperResStrength = 120;
            TrimStart = 0;
            ChangeSpeedValue = 100;
            VideoCodec = VideoCodecs.x265;
            EncodeQuality = 23;
            EncodePreset = EncodePresets.medium;
            AudioQuality = 50;
            ChangeAudioPitch = false;
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
            int ScaleFactor = 1;
            int FrameDoubleX = 0, FrameDoubleY = 0;
            int ScaleFactorX = 1, ScaleFactorY = 1;
            // Calculate scale factor based on height
            while ((SourceHeight - CropSource.Top - CropSource.Bottom) * ScaleFactorY < OutputHeight) {
                FrameDoubleY += 1;
                ScaleFactorY *= 2;
            }
            // Calculate scale factor based on width
            while ((SourceWidth - CropSource.Left - CropSource.Right) * ScaleFactorX * SourceAspectRatio < OutputWidth) {
                FrameDoubleX += 1;
                ScaleFactorX *= 2;
            }
            // Take highest ratio
            FrameDouble = FrameDoubleX > FrameDoubleY ? FrameDoubleX : FrameDoubleY;
            ScaleFactor = ScaleFactorX > ScaleFactorY ? ScaleFactorX : ScaleFactorY;

            if (Crop) {
                CropAfter = new Rect(
                    (CropLeft - CropSource.Left) * ScaleFactor,
                    (CropTop - CropSource.Top) * ScaleFactor,
                    (CropRight - CropSource.Right) * ScaleFactor,
                    (CropBottom - CropSource.Bottom) * ScaleFactor);
            } else
                CropAfter = new Rect();
            int CropHeight = ((SourceHeight.Value - CropSource.Top - CropSource.Bottom) * ScaleFactor);
            int CropWidth = ((SourceWidth.Value - CropSource.Left - CropSource.Right) * ScaleFactor);
            if (CropAfter.HasValue) {
                CropHeight = CropHeight - CropAfter.Top - CropAfter.Bottom;
                CropWidth = CropWidth - CropAfter.Left - CropAfter.Right;
            }
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

        public bool CanEncodeMp4 {
            get {
                return IsAudioMp4 || AudioAction != AudioActions.Copy;
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
                return audioAction == AudioActions.Ignore || audioAction == AudioActions.Encode;
            }
        }

        public string FileExtension {
            get {
                return EncodeFormat == VideoFormats.Mp4 ? "mp4" : "mkv";
            }
        }

        public bool HasFileName {
            get { return !string.IsNullOrEmpty(FileName); }
        }

        public string ScriptFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Script.avs", JobIndex); }
        }

        public string SettingsFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Settings.xml", JobIndex); }
        }

        public string InputFile {
            get {
                if (ConvertToAvi)
                    return Settings.TempFilesPath + string.Format("Job{0}_Input.avi", JobIndex);
                else
                    return Settings.NaturalGroundingFolder + FileName;
            }
        }

        public string OutputFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.mkv", JobIndex, VideoCodec == VideoCodecs.x265 ? "265" : "264"); }
        }

        public string AudioFileWav {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.wav", JobIndex); }
        }

        public string AudioFileAac {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.aac", JobIndex); }
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

        public object Clone() {
            return (MediaEncoderSettings)DataHelper.DeepClone(this);
        }
    }

    public enum ColorMatrix {
        Rec601,
        Rec709
    }

    public enum UpscaleMethods {
        SuperXbr,
        NNedi3
    }

    public enum VideoCodecs {
        Copy,
        x264,
        x265
    }

    public enum VideoFormats {
        Mp4,
        Mkv
    }

    public enum AudioActions {
        Copy,
        Ignore,
        Encode
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
