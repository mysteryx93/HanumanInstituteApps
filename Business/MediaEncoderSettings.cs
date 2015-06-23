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
        public bool FixColors { get; set; }
        public bool DoubleNNEDI3Before { get; set; }
        public bool DoubleEEDI3 { get; set; }
        public bool DoubleNNEDI3 { get; set; }
        public bool Resize { get; set; }
        public int ResizeHeight { get; set; }
        public bool Denoise { get; set; }
        public int DenoiseStrength { get; set; }
        public bool SharpenAfterDouble { get; set; }
        public int SharpenAfterDoubleStrength { get; set; }
        public bool SharpenFinal { get; set; }
        public int SharpenFinalStrength { get; set; }
        public bool IncreaseFrameRate { get; set; }
        public FrameRateModeEnum IncreaseFrameRateValue { get; set; }
        public int EncodeQuality { get; set; }
        public bool Crop { get; set; }
        public int CropLeft { get; set; }
        public int CropTop { get; set; }
        public int CropRight { get; set; }
        public int CropBottom { get; set; }
        public bool EncodeMp4 { get; set; }
        public bool Trim { get; set; }
        [DefaultValue(null)]
        public int? TrimStart { get; set; }
        [DefaultValue(null)]
        public int? TrimEnd { get; set; }
        public bool ChangeSpeed { get; set; }
        public int ChangeSpeedValue { get; set; }
        public int JobIndex { get; set; }

        public MediaEncoderSettings() {
            SourceAspectRatio = 1;
            ResizeHeight = 720;
            EncodeQuality = 25;
            DenoiseStrength = 20;
            SharpenFinalStrength = 3;
            ChangeSpeedValue = 100;
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
            get { return Settings.TempFilesPath + string.Format("Job{0}_Input.avi", JobIndex); }
        }

        public string OutputFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Output.h264", JobIndex); }
        }

        public string FinalFile {
            get { return Settings.TempFilesPath + string.Format("Job{0}_Final.{1}", JobIndex, EncodeMp4 ? "mp4" : "mkv"); }
        }

        public object Clone() {
            return (MediaEncoderSettings)DataHelper.DeepClone(this);
        }
    }
}
