using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.PowerliminalsPlayer.Models
{
    public class PresetItem : DependencyObject
    {
        public string Name { get; set; } = string.Empty;
        [XmlElement("File")]
        public ObservableCollection<FileItem> Files { get; } = new ObservableCollection<FileItem>();
        private readonly DispatcherTimer _volumeChangeTimer = new DispatcherTimer();

        public PresetItem()
        {
            _volumeChangeTimer.Interval = TimeSpan.FromMilliseconds(100);
            _volumeChangeTimer.Tick += VolumeChangeTimer_Tick;
        }

        public PresetItem(string name) : this()
        {
            Name = name;
        }

        private void VolumeChangeTimer_Tick(object? sender, EventArgs e)
        {
            _volumeChangeTimer.Stop();
            foreach (var item in Files)
            {
                item.AdjustVolume(MasterVolume);
            }
        }

        public static readonly DependencyProperty MasterVolumeProperty = DependencyProperty.Register("MasterVolume", typeof(double), typeof(PresetItem),
            new PropertyMetadata(100.0, OnMasterVolumeChanged));
        public double MasterVolume { get => (double)GetValue(MasterVolumeProperty); set => SetValue(MasterVolumeProperty, value); }
        private static void OnMasterVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (PresetItem)d;
            //D.volumeChangeTimer.Stop();
            p._volumeChangeTimer.Start();
        }

        /// <summary>
        /// Copies this preset into specified preset object.
        /// </summary>
        public void SaveAs(PresetItem dst)
        {
            dst.CheckNotNull(nameof(dst));

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
