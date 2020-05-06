using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Contains the application settings and configured playlists.
    /// </summary>
    [Serializable]
    [XmlRoot("Player432hz")]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class SettingsData
    {
        /// <summary>
        /// Gets or sets the list of configured playlists.
        /// </summary>
        [ValidateObject]
        [XmlElement("Playlist")]
        public List<SettingsPlaylistItem> Playlists { get; } = new List<SettingsPlaylistItem>();
        /// <summary>
        /// Gets or sets the width of the main window.
        /// </summary>
        [Range(560, 10000)]
        public double Width { get; set; } = 583;
        /// <summary>
        /// Gets or sets the height of the main window.
        /// </summary>
        [Range(240, 10000)]
        public double Height { get; set; } = 390;
        [Range(0, 10000)]
        public double Top { get; set; }
        [Range(0, 10000)]
        public double Left { get; set; }
        /// <summary>
        /// Gets or sets the playback volume.
        /// </summary>
        [Range(0, 100)]
        public int Volume { get; set; } = 100;
    }
}
