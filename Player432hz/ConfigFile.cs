using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Business;
using System.ComponentModel;

namespace Player432hz {
    [Serializable]
    [XmlRoot("Player432hz")]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class ConfigFile {
        [XmlElement("Playlist")]
        public ObservableCollection<PlaylistItem> Playlists = new ObservableCollection<PlaylistItem>();
        public double Width { get; set; }
        public double Height { get; set; }
        public int Volume { get; set; }

        /// <summary>
        /// Loads settings from the XML file
        /// </summary>
        /// <returns>The object created from the xml file</returns>
        public static ConfigFile Load() {
            if (File.Exists(Settings.Player432hzConfigFile)) {
                try {
                    using (var stream = System.IO.File.OpenRead(Settings.Player432hzConfigFile)) {
                        var serializer = new XmlSerializer(typeof(ConfigFile));
                        ConfigFile Result = serializer.Deserialize(stream) as ConfigFile;
                        return Result;
                    }
                } catch {
                }
            }
            return new Player432hz.ConfigFile();
        }

        /// <summary>
        /// Saves to an xml file
        /// </summary>
        public void Save() {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.Player432hzConfigFile));

            using (var writer = new System.IO.StreamWriter(Settings.Player432hzConfigFile)) {
                var serializer = new XmlSerializer(typeof(ConfigFile));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                serializer.Serialize(writer, this, ns);
                writer.Flush();
            }
        }
    }

    [Serializable]
    public class PlaylistItem {
        public string Name { get; set; }
        [XmlElement("Folder")]
        public ObservableCollection<string> Folders { get; } = new ObservableCollection<string>();

        public PlaylistItem() {
        }

        public PlaylistItem(string name) {
            this.Name = name;
        }
    }
}
