using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    // Full documentation of parameters here
    // http://www.guthspot.se/video/deshaker.htm
    [PropertyChanged.ImplementPropertyChanged]
    [Serializable()]
    public class MediaEncoderDeshakerSettings {
        public int Version { get; set; } = 19;
        public int Pass { get; set; } = 1;
        public int BlockSize { get; set; } = 30;
        public int DifferentialSearchRange { get; set; } = 4;
        public float SourcePixelAspectRatio { get; set; } = 1;
        private int SourcePixelAspectSelection { get; set; } = 0; // unused
        private int DestinationPixelAspectValue { get; set; } = 1; // ignored; same as source
        private int DestinationPixelAspectSelection { get; set; } = 0; // unused
        private int DestinationWidth { get; set; } = 640; // ignored; same as source
        private int DestinationHeight { get; set; } = 640; // ignored; same as source
        public int Scale { get; set; } = 1;
        public int UsePixels { get; set; } = 2;
        public int MotionSmoothPanX { get; set; } = 1000;
        public int MotionSmoothPanY { get; set; } = 1000;
        public int MotionSmoothRotation { get; set; } = 1000;
        public int MotionSmoothZoom { get; set; } = 1000;
        public int DiscardMotion { get; set; } = 4; // Discard motion of blocks that move more than X pixels in wrong direction
        private int VideoOutput { get; set; } = 0;
        public EdgeCompensationMethods EdgeCompensation { get; set; } = EdgeCompensationMethods.None;
        public int Resampling { get; set; } = 2;
        public int SkipGoodFrame { get; set; } = 8;
        public int InitialSearchRange { get; set; } = 30;
        public int DiscardBadMotion { get; set; } = 300; // Discard motion of blocks that have match value less than X
        public int DiscardBadMotion2 { get; set; } = 4; // Discard motion of blocks that have 2nd best match larger than best minus X
        public string LogFile { get; set; } = "";
        private bool AppendToFile { get; set; } = false;
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
        public int DeepAnalysis { get; set; } = 0; // Deep analysis if less than X percent of vectors are ok (Only used if Camcorder has a rolling shutter is set to 0.)
        public bool RollingShutter { get; set; } = false;
        private bool GenerateInterlacedProgressiveVideo { get; set; } = false;
        public bool UseSourceProperties { get; set; } = true;
        public bool SoftBorders { get; set; } = false;
        public bool ExtrapolateColorsIntoBorder { get; set; } = false;
        public int SoftBorderEdgeTransitionWidth { get; set; } = 10;
        public int DiscardMotionAbove { get; set; } = 1000;
        public bool RememberDiscardedAreasToNextFrame { get; set; } = true;
        public int RollingShutterAmount { get; set; } = 88;
        public bool DetectRotation { get; set; } = true;
        public bool DetectZoom { get; set; } = true;
        public int DetectScenesTreshold { get; set; } = 20;
        public int AdaptiveZoomSmoothness { get; set; } = 5000;
        public int AdaptiveZoomAmount { get; set; } = 100;
        public int DiscardMotionDifferenceMin { get; set; } = 20;
        public bool DetectScenes { get; set; } = true;
        public bool UseColorMask { get; set; } = false;
        public string MaskColor { get; set; } = "ff00ff";

        public bool FillBordersWithPreviousOrFutureFrames {
            get {
                return FillBordersWithPreviousFrames || FillBordersWithFutureFrames;
            }
        }

        public override string ToString() {
            StringBuilder ParamChain = new StringBuilder();
            for (int i = 0; i < 66; i++) {
                if (i > 0)
                    ParamChain.Append("|");
                ParamChain.Append("{");
                ParamChain.Append(i);
                ParamChain.Append("}");
            }
            return string.Format(ParamChain.ToString(),
                Version,
                Pass,
                BlockSize,
                DifferentialSearchRange,
                SourcePixelAspectRatio,
                SourcePixelAspectSelection,
                DestinationPixelAspectValue,
                DestinationPixelAspectSelection,
                DestinationWidth,
                DestinationHeight,
                Scale,
                UsePixels,
                MotionSmoothPanX,
                MotionSmoothPanY,
                MotionSmoothRotation,
                MotionSmoothZoom,
                DiscardMotion,
                VideoOutput,
                (int)EdgeCompensation,
                Resampling,
                SkipGoodFrame,
                InitialSearchRange,
                DiscardBadMotion,
                DiscardBadMotion2,
                LogFile,
                AppendToFile ? 1 : 0,
                IgnoreOutside ? 1 : 0,
                IgnoreOutsideLeft,
                IgnoreOutsideRight,
                IgnoreOutsideTop,
                IgnoreOutsideBottom,
                IgnoreInside ? 1 : 0,
                IgnoreInsideLeft,
                IgnoreInsideRight,
                IgnoreInsideTop,
                IgnoreInsideBottom,
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
                DeepAnalysis,
                RollingShutter ? 1 : 0,
                GenerateInterlacedProgressiveVideo ? 1 : 0,
                UseSourceProperties ? 1 : 0,
                SoftBorders ? 1 : 0,
                ExtrapolateColorsIntoBorder ? 1 : 0,
                SoftBorderEdgeTransitionWidth,
                DiscardMotionAbove,
                RememberDiscardedAreasToNextFrame ? 1 : 0,
                RollingShutterAmount,
                DetectRotation ? 1 : 0,
                DetectZoom ? 1 : 0,
                DetectScenesTreshold,
                AdaptiveZoomSmoothness,
                AdaptiveZoomAmount,
                DiscardMotionDifferenceMin,
                DetectScenes ? 1 : 0,
                UseColorMask ? 1 : 0,
                MaskColor);
        }

        public MediaEncoderDeshakerSettings Clone() {
            return (MediaEncoderDeshakerSettings)this.MemberwiseClone();
        }
   }

    public enum EdgeCompensationMethods {
        None = 0,
        AdaptiveZoomAverage = 1,
        FixedZoom = 3,
        AdaptiveZoomAverageFixedZoom = 4,
        AdaptiveZoomFull = 6
    }
}