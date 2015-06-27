using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    [PropertyChanged.ImplementPropertyChanged]
    public class VideoListItem {
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
        //public double? PhysicalMasculine { get; set; }
        //public double? PhysicalFeminine { get; set; }
        //public double? EmotionalMasculine { get; set; }
        //public double? EmotionalFeminine { get; set; }
        //public double? SpiritualMasculine { get; set; }
        //public double? SpiritualFeminine { get; set; }
        //public double? Love { get; set; }
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
                else if (status == VideoListItemStatusEnum.DownloadingInfo)
                    StatusText = "Downloading Info";
                else if (status == VideoListItemStatusEnum.Downloading)
                    StatusText = "Downloading";
                else if (status == VideoListItemStatusEnum.Done)
                    StatusText = "Done";
                else if (status == VideoListItemStatusEnum.Failed)
                    StatusText = "Download Failed";
            }
        }
        public string StatusText { get; set; }

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
