using System;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    public enum VideoListItemStatus
    {
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
