using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public enum MediaType {
        None = -1,
        Video = 0,
        Audio = 1,
        Image = 2
    }

    public enum HasRatingEnum {
        All = 0,
        With = 1,
        Without = 2,
        Incomplete = 3
    }

    public enum SearchFilterEnum {
        None,
        All,
        Artist,
        Category,
        Element,
        Files,
        ArtistSingles
    }

    public enum FieldConditionEnum {
        None,
        FileExists,
        HasDownloadUrl,
        HasBuyOrDownloadUrl,
        PerformanceProblem,
        IsPersonal
    }

    public enum BoolConditionEnum {
        None = 0,
        Yes = 1,
        No = 2
    }

    public enum OperatorConditionEnum {
        GreaterOrEqual,
        Equal,
        Smaller
    }

    public enum FrameRateModeEnum {
        Double,
        fps30,
        fps60,
        fps120
    }

    public enum VideoListItemStatusEnum {
        None,
        OK,
        InvalidUrl,
        HigherQualityAvailable,
        BetterAudioAvailable,
        BetterVideoAvailable,
        WrongContainer,
        DownloadingInfo,
        Downloading,
        Converting,
        Done,
        Failed
    }
}
