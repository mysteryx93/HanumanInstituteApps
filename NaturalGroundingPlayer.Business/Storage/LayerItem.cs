using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {
    /// <summary>
    /// Represents a media layer added to the UI.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class LayerItem {
        /// <summary>
        /// Gets or sets the type of media.
        /// </summary>
        public MediaType Type { get; set; }
        /// <summary>
        /// Gets or sets the file name of the media to display.
        /// </summary>
        public string FileName { get; set; }
    }
}
