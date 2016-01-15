using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    public enum MediaPlayerApplication {
        Mpc = 0,
        Wmp = 1
    }

    public enum DownloadQuality {
        Max = 0,
        p1080 = 1,
        p720 = 2,
        p480 = 3,
        p360 = 4,
        p240 = 5
    }

    public enum AviSynthVersion {
        None,
        AviSynth26,
        AviSynthPlus
    }
}
