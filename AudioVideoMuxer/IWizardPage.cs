using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioVideoMuxer {
    public interface IWizardPage {
        MainWindow Owner { get; set; }
        bool Validate();
    }
}
