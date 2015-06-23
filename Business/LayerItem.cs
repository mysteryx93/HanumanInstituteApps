using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    [PropertyChanged.ImplementPropertyChanged]
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
