using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    public class PresetItem : DependencyObject
    {
        public string Name { get; set; }
        [XmlElement("File")]
        public ObservableCollection<FileItem> Files { get; } = new ObservableCollection<FileItem>();
        private DispatcherTimer volumeChangeTimer = new DispatcherTimer();

        public PresetItem()
        {
            volumeChangeTimer.Interval = TimeSpan.FromMilliseconds(100);
            volumeChangeTimer.Tick += VolumeChangeTimer_Tick;
        }

        public PresetItem(string name) : this()
        {
            this.Name = name;
        }

        private void VolumeChangeTimer_Tick(object sender, EventArgs e)
        {
            volumeChangeTimer.Stop();
            foreach (FileItem item in Files)
            {
                item.AdjustVolume(MasterVolume);
            }
        }

        public static readonly DependencyProperty MasterVolumeProperty =
            DependencyProperty.Register("MasterVolume", typeof(double),
            typeof(PresetItem), new PropertyMetadata(100.0, OnMasterVolumeChanged));

        public double MasterVolume
        {
            get
            {
                return (double)GetValue(MasterVolumeProperty);
            }
            set
            {
                SetValue(MasterVolumeProperty, value);
            }
        }

        private static void OnMasterVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PresetItem D = d as PresetItem;
            //D.volumeChangeTimer.Stop();
            D.volumeChangeTimer.Start();
        }

        /// <summary>
        /// Copies this preset into specified preset object.
        /// </summary>
        public void SaveAs(PresetItem dst)
        {
            dst.Name = Name;
            dst.MasterVolume = MasterVolume;
            dst.Files.Clear();
            foreach (var item in Files)
            {
                dst.Files.Add(item.Clone());
            }
        }
    }
}
