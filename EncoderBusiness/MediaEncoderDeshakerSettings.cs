using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EmergenceGuardian.MediaEncoder {
    // Full documentation of parameters here
    // http://www.guthspot.se/video/deshaker.htm
    [Serializable()]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class MediaEncoderDeshakerSettings {
        public int Version { get; set; } = 19;
        public int Pass { get; set; } = 1;
        public float SourcePixelAspectRatio { get; set; } = 1;
        private int SourcePixelAspectSelection { get; set; } = 0; // unused
        private int DestinationPixelAspectValue { get; set; } = 1; // ignored; same as source
        private int DestinationPixelAspectSelection { get; set; } = 0; // unused
        private int DestinationWidth { get; set; } = 640; // ignored; same as source
        private int DestinationHeight { get; set; } = 640; // ignored; same as source
        public int MotionSmoothPanX { get; set; } = 1000;
        public int MotionSmoothPanY { get; set; } = 1000;
        public int MotionSmoothRotation { get; set; } = 1000;
        public int MotionSmoothZoom { get; set; } = 1000;
        private int VideoOutput { get; set; } = 0;
        public EdgeCompensationMethods EdgeCompensation { get; set; } = EdgeCompensationMethods.AdaptiveZoomFull;
        public int Resampling { get; set; } = 2;
        public string LogFile { get; set; } = "";
        public bool AppendToFile { get; set; } = false;
        private bool Interlaced { get; set; } = false; // we always deinterlace first
        private bool InterlacedUpperFieldFirst { get; set; } = false;
        public int ExtraZoomFactor { get; set; } = 1;
        public int MaxPanX { get; set; } = 15;
        public int MaxPanY { get; set; } = 15;
        public int MaxRotation { get; set; } = 5;
        public int MaxZoom { get; set; } = 15;
        public bool FillBordersWithPreviousFrames { get; set; } = false;
        public bool FillBordersWithFutureFrames { get; set; } = false;
        public int FillBordersWithPreviousFramesCount { get; set; } = 30;
        public int FillBordersWithFutureFramesCount { get; set; } = 30;
        public bool IgnoreOutsideLetAreaFollowMotion { get; set; } = false;
        private bool GenerateInterlacedProgressiveVideo { get; set; } = false;
        public bool UseSourceProperties { get; set; } = true;
        public bool SoftBorders { get; set; } = false;
        public bool ExtrapolateColorsIntoBorder { get; set; } = false;
        public int SoftBorderEdgeTransitionWidth { get; set; } = 10;
        public int AdaptiveZoomSmoothness { get; set; } = 5000;
        public int AdaptiveZoomAmount { get; set; } = 100;
        public bool UseColorMask { get; set; } = false;
        public string MaskColor { get; set; } = "ff00ff";

        public PrescanType PrescanAction { get; set; }
        public short? PrescanStart { get; set; }
        public short? PrescanEnd { get; set; }
        [XmlIgnore()]
        public bool PrescanCompleted { get; set; }

        public ObservableCollection<MediaEncoderDeshakerSegmentSettings> Segments { get; set; }

        public bool FillBordersWithPreviousOrFutureFrames {
            get {
                return FillBordersWithPreviousFrames || FillBordersWithFutureFrames;
            }
        }

        public override string ToString() {
            return ToString(0);
        }

        public string ToString(int segment) {
            if (segment < 0 || segment >= Segments.Count())
                throw new ArgumentException("Invalid segment", "segment");

            StringBuilder ParamChain = new StringBuilder();
            for (int i = 0; i < 66; i++) {
                if (i > 0)
                    ParamChain.Append("|");
                ParamChain.Append("{");
                ParamChain.Append(i);
                ParamChain.Append("}");
            }
            MediaEncoderDeshakerSegmentSettings s = Segments[segment];
            return string.Format(ParamChain.ToString(),
                Version,
                Pass,
                s.BlockSize,
                s.DifferentialSearchRange,
                SourcePixelAspectRatio,
                SourcePixelAspectSelection,
                DestinationPixelAspectValue,
                DestinationPixelAspectSelection,
                DestinationWidth,
                DestinationHeight,
                (int)s.Scale,
                (int)s.UsePixels,
                MotionSmoothPanX,
                MotionSmoothPanY,
                MotionSmoothRotation,
                MotionSmoothZoom,
                s.DiscardMotion,
                VideoOutput,
                (int)EdgeCompensation,
                Resampling,
                s.SkipBadFrame,
                s.InitialSearchRange,
                s.DiscardBadMotion,
                s.DiscardBadMotion2,
                LogFile,
                AppendToFile ? 1 : 0,
                s.IgnoreOutside ? 1 : 0,
                s.IgnoreOutsideLeft,
                s.IgnoreOutsideRight,
                s.IgnoreOutsideTop,
                s.IgnoreOutsideBottom,
                s.IgnoreInside ? 1 : 0,
                s.IgnoreInsideLeft,
                s.IgnoreInsideRight,
                s.IgnoreInsideTop,
                s.IgnoreInsideBottom,
                Interlaced ? 1 : 0,
                InterlacedUpperFieldFirst ? 1 : 0,
                ExtraZoomFactor,
                MaxPanX,
                MaxPanY,
                MaxRotation,
                MaxZoom,
                FillBordersWithPreviousFrames ? 1 : 0,
                FillBordersWithFutureFrames ? 1 : 0,
                FillBordersWithPreviousFramesCount,
                FillBordersWithFutureFramesCount,
                IgnoreOutsideLetAreaFollowMotion ? 1 : 0,
                s.DeepAnalysis,
                s.RollingShutter ? 1 : 0,
                GenerateInterlacedProgressiveVideo ? 1 : 0,
                UseSourceProperties ? 1 : 0,
                SoftBorders ? 1 : 0,
                ExtrapolateColorsIntoBorder ? 1 : 0,
                SoftBorderEdgeTransitionWidth,
                s.DiscardAbsoluteMotion,
                s.RememberDiscardedAreasToNextFrame ? 1 : 0,
                s.RollingShutterAmount,
                s.DetectRotation ? 1 : 0,
                s.DetectZoom ? 1 : 0,
                s.DetectScenesTreshold,
                AdaptiveZoomSmoothness,
                AdaptiveZoomAmount,
                s.DiscardMotionDiff,
                s.DetectScenes ? 1 : 0,
                UseColorMask ? 1 : 0,
                MaskColor);
        }

        public MediaEncoderDeshakerSettings Clone() {
            return (MediaEncoderDeshakerSettings)this.DeepClone();
        }

        public void CopyTo(MediaEncoderDeshakerSettings target) {
            ExtensionMethods.CopyAll<MediaEncoderDeshakerSettings>(this, target);
        }
    }

    [Serializable()]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class MediaEncoderDeshakerSegmentSettings {
        public int FrameStart { get; set; } = 0; // To apply settings for different segments of the clip
        public int BlockSize { get; set; } = 30;
        public int DifferentialSearchRange { get; set; } = 4;
        public DeshakerScales Scale { get; set; } = DeshakerScales.Half;
        public DeshakerUsePixels UsePixels { get; set; } = DeshakerUsePixels.Every4;
        public int DiscardMotion { get; set; } = 4; // Discard motion of blocks that move more than X pixels in wrong direction
        public int SkipBadFrame { get; set; } = 8;
        public int InitialSearchRange { get; set; } = 30;
        public int DiscardBadMotion { get; set; } = 300; // Discard motion of blocks that have match value less than X
        public int DiscardBadMotion2 { get; set; } = 4; // Discard motion of blocks that have 2nd best match larger than best minus X
        public bool IgnoreOutside { get; set; } = false;
        public int IgnoreOutsideLeft { get; set; } = 0;
        public int IgnoreOutsideRight { get; set; } = 0;
        public int IgnoreOutsideTop { get; set; } = 0;
        public int IgnoreOutsideBottom { get; set; } = 0;
        public bool IgnoreInside { get; set; } = false;
        public int IgnoreInsideLeft { get; set; } = 0;
        public int IgnoreInsideRight { get; set; } = 0;
        public int IgnoreInsideTop { get; set; } = 0;
        public int IgnoreInsideBottom { get; set; } = 0;
        public int DeepAnalysis { get; set; } = 0; // Deep analysis if less than X percent of vectors are ok (Only used if Camcorder has a rolling shutter is set to 0.)
        public bool RollingShutter { get; set; } = false;
        public int DiscardAbsoluteMotion { get; set; } = 1000;
        public bool RememberDiscardedAreasToNextFrame { get; set; } = true;
        public int RollingShutterAmount { get; set; } = 88;
        public bool DetectRotation { get; set; } = true;
        public bool DetectZoom { get; set; } = true;
        public int DetectScenesTreshold { get; set; } = 20;
        public int DiscardMotionDiff { get; set; } = 20;
        public bool DetectScenes { get; set; } = true;

        public MediaEncoderDeshakerSegmentSettings Clone() {
            return (MediaEncoderDeshakerSegmentSettings)MemberwiseClone();
        }

        public void CopyTo(MediaEncoderDeshakerSegmentSettings target) {
            ExtensionMethods.CopyAll<MediaEncoderDeshakerSegmentSettings>(this, target);
        }
    }

    public enum EdgeCompensationMethods {
        None = 0,
        AdaptiveZoomAverage = 1,
        FixedZoom = 3,
        AdaptiveZoomAverageFixedZoom = 4,
        AdaptiveZoomFull = 6
    }

    public enum DeshakerScales {
        Full = 0,
        Half = 1,
        Quarter = 2
    }

    public enum DeshakerUsePixels {
        All = 1,
        Every4 = 2,
        Every9 = 3,
        Every16 = 4
    }

    public enum PrescanType {
        Full,
        Preview
    }

    public enum LogGenerationStatus {
        None,
        Partial,
        Full,
        Working
    }
}