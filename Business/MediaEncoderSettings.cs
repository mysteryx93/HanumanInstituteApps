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
        [DefaultValue(null)]
        public string SourceVideoFormat { get; set; }
        public ColorMatrix SourceColorMatrix { get; set; }
        public int OutputHeight { get; set; }
        public bool Denoise1 { get; set; }
        public int Denoise1Strength { get; set; }
        public bool Denoise2 { get; set; }
        public int Denoise2Strength { get; set; }
        public int Denoise2Sharpen { get; set; }
        public bool SuperRes { get; set; }
        public bool SuperResDoublePass { get; set; }
        public int SuperResStrength { get; set; }
        public int SuperResSoftness { get; set; }
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

        public string CustomScript { get; set; }
        public int JobIndex { get; set; }

        public MediaEncoderSettings() {
            OpenMethod = OpenMethods.ConvertToAvi;
            SourceAspectRatio = 1;
            SourceColorMatrix = ColorMatrix.Rec601;
            OutputHeight = 720;
            Denoise1 = true;
            Denoise1Strength = 30;
            Denoise2Strength = 30;
            Denoise2Sharpen = 10;
            SuperRes = true;
            SuperResDoublePass = true;
            SuperResStrength = 43;
            SuperResSoftness = 0;
            IncreaseFrameRate = true;
            IncreaseFrameRateValue = FrameRateModeEnum.fps60;
            ChangeSpeedValue = 100;
            EncodeQuality = 24;
            EncodePreset = EncodePresets.veryslow;
            AudioQuality = 50;
            ChangeAudioPitch = false;
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
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.mp4", JobIndex); }
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
            get { return Settings.TempFilesPath + string.Format("Job{0}_Temp", JobIndex); }
        }

        public object Clone() {
            return (MediaEncoderSettings)DataHelper.DeepClone(this);
        }
    }

    public enum OpenMethods {
        ConvertToAvi,
        Direct
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
}
