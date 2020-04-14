using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using PropertyChanged;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    [AddINotifyPropertyChangedInterface]
    public class FileItem : DependencyObject
    {
        [XmlIgnore]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullPath { get; set; }
        public string FileName
        {
            get { return System.IO.Path.GetFileName(FullPath); }
        }
        [XmlElement("Volume")]
        public double FullVolume { get; set; } = 100;
        private double MasterVolume { get; set; } = -1;
        private bool adjustingVolume { get; set; } = false;

        public bool IsPlaying { get; set; } = false;

        public FileItem()
        {
        }

        public FileItem(string path, double masterVolume)
        {
            this.FullPath = path;
            // this.MasterVolume = masterVolume;
            AdjustVolume(masterVolume);
        }

        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(FileItem),
            new PropertyMetadata(100.0, OnVolumeChanged));
        [XmlIgnore]
        public double Volume { get => (double)GetValue(VolumeProperty); set => SetValue(VolumeProperty, value); }
        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileItem Item = d as FileItem;
            if (!Item.adjustingVolume && Item.MasterVolume > 0)
                Item.FullVolume = ((double)e.NewValue) * 100 / Item.MasterVolume;
        }

        public void AdjustVolume(double newMasterVolume)
        {
            if (this.MasterVolume != newMasterVolume)
            {
                adjustingVolume = true;
                Volume = FullVolume * newMasterVolume / 100;
                this.MasterVolume = newMasterVolume;
                adjustingVolume = false;
            }
        }

        private int speed;
        public int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                double Factor = value / 8.0;
                Rate = Factor < 0 ? 1 / (1 - Factor) : 1 * (1 + Factor);
            }
        }

        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(double),
            typeof(FileItem), new PropertyMetadata(1.0));

        [XmlIgnore]
        public double Rate
        {
            get
            {
                return (double)GetValue(RateProperty);
            }
            set
            {
                SetValue(RateProperty, value);
            }
        }

        public FileItem Clone()
        {
            return (FileItem)MemberwiseClone();
        }
    }
}
