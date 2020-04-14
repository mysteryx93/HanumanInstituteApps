using System;
using System.Text;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class MediaListItem {
        public Guid? MediaId { get; set; }
        public MediaType MediaType { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public Guid? MediaCategoryId { get; set; }
        public string FileName { get; set; }
        public double? Preference { get; set; }
        public int? Length { get; set; }
        public bool HasDownloadUrl { get; set; }
        public string BuyUrl { get; set; }
        public double? Intensity { get; set; }
        public double? Egoless { get; set; }
        public double? Custom { get; set; }
        public bool FileExists { get; set; }
        public bool IsInDatabase { get; set; }
        private VideoListItemStatusEnum status;
        public VideoListItemStatusEnum Status {
            get {
                return status;
            }
            set {
                status = value;
                if (status == VideoListItemStatusEnum.None)
                    StatusText = "";
                else if (status == VideoListItemStatusEnum.OK)
                    StatusText = "OK";
                else if (status == VideoListItemStatusEnum.InvalidUrl)
                    StatusText = "Invalid Url";
                else if (status == VideoListItemStatusEnum.HigherQualityAvailable)
                    StatusText = "Higher Quality Available";
                else if (status == VideoListItemStatusEnum.BetterAudioAvailable)
                    StatusText = "Better Audio Available";
                else if (status == VideoListItemStatusEnum.BetterVideoAvailable)
                    StatusText = "Better Video Available";
                else if (status == VideoListItemStatusEnum.WrongContainer)
                    StatusText = "Wrong Container";
                else if (status == VideoListItemStatusEnum.DownloadingInfo)
                    StatusText = "Downloading Info";
                else if (status == VideoListItemStatusEnum.Downloading)
                    StatusText = "Downloading";
                else if (status == VideoListItemStatusEnum.Converting)
                    StatusText = "Converting";
                else if (status == VideoListItemStatusEnum.Done)
                    StatusText = "Done";
                else if (status == VideoListItemStatusEnum.Failed)
                    StatusText = "Download Failed";
                else
                    StatusText = "Unknown Status";
            }
        }
        public string StatusText { get; set; }
        public bool IsBusy { get; set; }

        public bool CanDownload {
            get {
                return status == VideoListItemStatusEnum.HigherQualityAvailable || status == VideoListItemStatusEnum.BetterAudioAvailable || status == VideoListItemStatusEnum.BetterVideoAvailable;
            }
        }

        public string BuyName {
            get {
                StringBuilder Result = new StringBuilder();
                Result.Append(Artist);
                if (!string.IsNullOrEmpty(Album)) {
                    Result.Append(" - ");
                    Result.Append(Album);
                }
                return Result.ToString();
            }
        }
    }
}
