using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    /// <summary>
    /// Contains the PowerliminalsPlayer application settings.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    [XmlRoot("PowerliminalsPlayer")]
    public class AppSettingsData
    {
        public AppSettingsData() { }

        /// <summary>
        /// Gets or sets the zoom factor to enlarge the UI.
        /// </summary>
        [Range(1.0, 1.5)]
        public double Zoom { get; set; } = 1;

        /// <summary>
        /// Gets or sets the list of folders in which to look for audio files.
        /// </summary>
        [XmlElement("Folder")]
        public ObservableCollection<string> Folders { get; } = new ObservableCollection<string>();
        /// <summary>
        /// Gets or sets the list of saved presets.
        /// </summary>
        public ObservableCollection<PresetItem> Presets { get; } = new ObservableCollection<PresetItem>();
        /// <summary>
        /// Gets or sets whether the folders section is expanded.
        /// </summary>
        public bool FoldersExpanded { get; set; } = true;
        /// <summary>
        /// Gets or sets the main window width.
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// Gets or sets the main window height.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Creates a copy of the SettingsFile class.
        /// </summary>
        /// <returns>The copied object.</returns>
        public AppSettingsData Clone() => (AppSettingsData)MemberwiseClone();
    }
}
