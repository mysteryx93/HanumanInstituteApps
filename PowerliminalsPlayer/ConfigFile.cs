using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Business;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

namespace PowerliminalsPlayer {
    [XmlRoot("PowerliminalsPlayer")]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class ConfigFile {
        [XmlElement("Folder")]
        public ObservableCollection<string> Folders = new ObservableCollection<string>();
        public ObservableCollection<PresetItem> Presets = new ObservableCollection<PresetItem>();
        public bool FoldersExpanded { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        [XmlIgnore]
        public PresetItem Current = new PresetItem();
        [XmlIgnore]
        internal ObservableCollection<string> Files = new ObservableCollection<string>();

        /// <summary>
        /// Loads settings from the XML file
        /// </summary>
        /// <returns>The object created from the xml file</returns>
        public static ConfigFile Load() {
            if (File.Exists(Settings.PowerliminalsPlayerConfigFile)) {
                try {
                    using (var stream = System.IO.File.OpenRead(Settings.PowerliminalsPlayerConfigFile)) {
                        var serializer = new XmlSerializer(typeof(ConfigFile));
                        ConfigFile Result = serializer.Deserialize(stream) as ConfigFile;
                        return Result;
                    }
                }
                catch {
                }
            }
            return new ConfigFile();
        }

        /// <summary>
        /// Saves to an xml file
        /// </summary>
        public void Save() {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.PowerliminalsPlayerConfigFile));

            using (var writer = new System.IO.StreamWriter(Settings.PowerliminalsPlayerConfigFile)) {
                var serializer = new XmlSerializer(typeof(ConfigFile));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                serializer.Serialize(writer, this, ns);
                writer.Flush();
            }
        }
    }

    public class PresetItem:DependencyObject {
        public string Name { get; set; }
        [XmlElement("File")]
        public ObservableCollection<FileItem> Files { get; } = new ObservableCollection<FileItem>();
        private DispatcherTimer volumeChangeTimer = new DispatcherTimer();

        public PresetItem() {
            volumeChangeTimer.Interval = TimeSpan.FromMilliseconds(100);
            volumeChangeTimer.Tick += VolumeChangeTimer_Tick;
        }

        public PresetItem(string name):this() {
            this.Name = name;
        }

        private void VolumeChangeTimer_Tick(object sender, EventArgs e) {
            volumeChangeTimer.Stop();
            foreach (FileItem item in Files) {
                item.AdjustVolume(MasterVolume);
            }
        }

        public static readonly DependencyProperty MasterVolumeProperty =
            DependencyProperty.Register("MasterVolume", typeof(double),
            typeof(PresetItem), new PropertyMetadata(100.0, OnMasterVolumeChanged));

        public double MasterVolume {
            get {
                return (double)GetValue(MasterVolumeProperty);
            }
            set {
                SetValue(MasterVolumeProperty, value);
            }
        }

        private static void OnMasterVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PresetItem D = d as PresetItem;
            //D.volumeChangeTimer.Stop();
            D.volumeChangeTimer.Start();
        }

        /// <summary>
        /// Copies this preset into specified preset object.
        /// </summary>
        public void SaveAs(PresetItem dst) {
            dst.Name = Name;
            dst.MasterVolume = MasterVolume;
            dst.Files.Clear();
            foreach (var item in Files) {
                dst.Files.Add(item.Clone());
            }
        }
    }

    public class FileItem : DependencyObject {
        [XmlIgnore]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullPath { get; set; }
        public string FileName {
            get { return System.IO.Path.GetFileName(FullPath); }
        }
        [XmlElement("Volume")]
        public double FullVolume { get; set; } = 100;
        private double MasterVolume { get; set; } = -1;
        private bool adjustingVolume { get; set; } = false;

        public FileItem() {
        }

        public FileItem(string path, double masterVolume) {
            this.FullPath = path;
            // this.MasterVolume = masterVolume;
            AdjustVolume(masterVolume);
        }

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double),
            typeof(FileItem), new PropertyMetadata(100.0, OnVolumeChanged));

        [XmlIgnore]
        public double Volume {
            get {
                return (double)GetValue(VolumeProperty);
            }
            set {
                SetValue(VolumeProperty, value);
            }
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FileItem Item = d as FileItem;
            if (!Item.adjustingVolume && Item.MasterVolume > 0)
                Item.FullVolume = ((double)e.NewValue) * 100 / Item.MasterVolume;
        }

        public void AdjustVolume(double newMasterVolume) {
            if (this.MasterVolume != newMasterVolume) {
                adjustingVolume = true;
                Volume = FullVolume * newMasterVolume / 100;
                this.MasterVolume = newMasterVolume;
                adjustingVolume = false;
            }
        }

        private int speed;
        public int Speed {
            get {
                return speed;
            }
            set {
                speed = value;
                double Factor = value / 8.0;
                Rate = Factor < 0 ? 1 / (1 - Factor) : 1 * (1 + Factor);
            }
        }

        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(double),
            typeof(FileItem), new PropertyMetadata(1.0));

        [XmlIgnore]
        public double Rate {
            get {
                return (double)GetValue(RateProperty);
            }
            set {
                SetValue(RateProperty, value);
            }
        }

        public FileItem Clone() {
            return (FileItem)MemberwiseClone();
        }
    }
}
