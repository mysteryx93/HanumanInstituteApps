using System;
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
        public string FullPath { get; set; } = string.Empty;
        public string FileName => System.IO.Path.GetFileName(FullPath);

        [XmlElement("Volume")]
        public double FullVolume { get; set; } = 100;
        private double MasterVolume { get; set; } = -1;
        private bool _adjustingVolume = false;

        public bool IsPlaying { get; set; } = false;

        public FileItem()
        { }

        public FileItem(string path, double masterVolume)
        {
            FullPath = path;
            // this.MasterVolume = masterVolume;
            AdjustVolume(masterVolume);
        }

        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(FileItem),
            new PropertyMetadata(100.0, OnVolumeChanged));
        [XmlIgnore]
        public double Volume { get => (double)GetValue(VolumeProperty); set => SetValue(VolumeProperty, value); }
        [SuppressPropertyChangedWarnings]
        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (FileItem)d;
            if (!item._adjustingVolume && item.MasterVolume > 0)
            {
                item.FullVolume = ((double)e.NewValue) * 100 / item.MasterVolume;
            }
        }

        public void AdjustVolume(double newMasterVolume)
        {
            if (MasterVolume != newMasterVolume)
            {
                _adjustingVolume = true;
                Volume = FullVolume * newMasterVolume / 100;
                MasterVolume = newMasterVolume;
                _adjustingVolume = false;
            }
        }

        private int _speed;
        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                var factor = value / 8.0;
                Rate = factor < 0 ? 1 / (1 - factor) : 1 * (1 + factor);
            }
        }

        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(double),
            typeof(FileItem), new PropertyMetadata(1.0));

        [XmlIgnore]
        public double Rate
        {
            get => (double)GetValue(RateProperty);
            set => SetValue(RateProperty, value);
        }

        public FileItem Clone() => (FileItem)MemberwiseClone();
    }
}
