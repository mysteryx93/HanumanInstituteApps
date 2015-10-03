using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Business {
    [PropertyChanged.ImplementPropertyChanged]
    [Serializable()]
    public class MediaEncoderSettings : ICloneable {
        public OpenMethods OpenMethod { get; set; }
        public string FileName { get; set; }
        [DefaultValue(null)]
        public double? Position { get; set; }
        [DefaultValue(null)]
        public int? SourceHeight { get; set; }
        [DefaultValue(null)]
        public int? SourceWidth { get; set; }
        public double SourceAspectRatio { get; set; }
        [DefaultValue(null)]
        public double? SourceFrameRate { get; set; }
        [DefaultValue(null)]
        public string SourceAudioFormat { get; set; }
        [DefaultValue(null)]
        public int? SourceAudioBitrate { get; set; }
        public ColorMatrix SourceColorMatrix { get; set; }
        public bool DoubleNNEDI3Before { get; set; }
        public bool DoubleEEDI3 { get; set; }
        public bool DoubleNNEDI3 { get; set; }
        public bool Resize { get; set; }
        public int ResizeHeight { get; set; }
        public bool Denoise1 { get; set; }
        public int Denoise1Strength { get; set; }
        public bool Denoise2 { get; set; }
        public int Denoise2Strength { get; set; }
        public int Denoise2Sharpen { get; set; }
        public bool SuperRes { get; set; }
        public int SuperResStrength { get; set; }
        public int SuperResSoftness { get; set; }
        public bool SharpenFinal { get; set; }
        public int SharpenFinalStrength { get; set; }
        public bool IncreaseFrameRate { get; set; }
        public FrameRateModeEnum IncreaseFrameRateValue { get; set; }
        public bool Crop { get; set; }
        public int CropLeft { get; set; }
        public int CropTop { get; set; }
        public int CropRight { get; set; }
        public int CropBottom { get; set; }
        public bool Trim { get; set; }
        [DefaultValue(null)]
        public int? TrimStart { get; set; }
        [DefaultValue(null)]
        public int? TrimEnd { get; set; }
        public bool ChangeSpeed { get; set; }
        public int ChangeSpeedValue { get; set; }

        public int EncodeQuality { get; set; }
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
        [DefaultValue(null)]
        public AudioBitrates EncodeAudioBitrate { get; set; }
        [DefaultValue(null)]
        public float? EncodeAudioGain { get; set; }

        public string CustomScript { get; set; }
        public int JobIndex { get; set; }

        public MediaEncoderSettings() {
            OpenMethod = OpenMethods.ConvertToAvi;
            SourceAspectRatio = 1;
            SourceColorMatrix = ColorMatrix.Rec709;
            ResizeHeight = 720;
            Denoise1Strength = 20;
            Denoise2Strength = 20;
            Denoise2Sharpen = 10;
            SharpenFinalStrength = 3;
            ChangeSpeedValue = 100;
            EncodeQuality = 25;
            EncodePreset = EncodePresets.veryslow;
            EncodeAudioBitrate = AudioBitrates.b256;
        }

        public bool CanEncodeMp4 {
            get {
                return IsAudioMp4 || AudioAction != AudioActions.Copy;
            }
        }

        public bool IsAudioMp4 {
            get {
                return SourceAudioFormat == "AAC" || SourceAudioFormat == "AC3" || SourceAudioFormat == "MP3" || string.IsNullOrEmpty(SourceAudioFormat);
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

        public bool ConvertToAvi {
            get {
                return OpenMethod == OpenMethods.ConvertToAvi;
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
                if (OpenMethod == OpenMethods.ConvertToAvi)
                    return Settings.TempFilesPath + string.Format("Job{0}_Input.avi", JobIndex);
                else
                    return Settings.NaturalGroundingFolder + FileName;
            }
        }

        public string OutputFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.264", JobIndex); }
        }

        public string FinalFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Final.{1}", JobIndex, EncodeFormat == VideoFormats.Mp4 ? "mp4" : "mkv"); }
        }

        public object Clone() {
            return (MediaEncoderSettings)DataHelper.DeepClone(this);
        }
    }

    public enum OpenMethods {
        ConvertToAvi,
        DirectShowSource
    }

    public enum ColorMatrix {
        Rec601,
        Rec709
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
        medium,
        slow,
        slower,
        veryslow,
        placebo
    }

    public enum AudioBitrates {
        b64,
        b96,
        b128,
        b192,
        b256,
        b384
    }
}
