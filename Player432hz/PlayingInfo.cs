using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player432hz {
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class PlayingInfo {
        public string FileName { get; set; }
        public double Position { get; set; }
        public double Duration { get; set; }
    }
}
