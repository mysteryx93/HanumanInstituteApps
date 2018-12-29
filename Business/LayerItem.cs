using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class LayerItem {
        public LayerType Type { get; set; }
        public string FileName { get; set; }
    }

    public enum LayerType {
        Audio,
        Video,
        Image
    }
}
