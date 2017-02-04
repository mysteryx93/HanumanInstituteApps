using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player432hz {
    [PropertyChanged.ImplementPropertyChanged]
    public class PlayingInfo {
        public string FileName { get; set; }
        public double Position { get; set; }
        public double Duration { get; set; }
    }
}
