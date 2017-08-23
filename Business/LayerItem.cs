using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    public class LayerItem : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public LayerType Type { get; set; }
        public string FileName { get; set; }
    }

    public enum LayerType {
        Audio,
        Video,
        Image
    }
}
