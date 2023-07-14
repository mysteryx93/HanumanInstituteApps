using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DataAccess;

namespace NaturalGroundingPlayer {
    public interface ILayer {
        void Close();
        void Hide();
        void Show();
        event EventHandler Closing;
    }

    public interface ILayerContainer {
        void AdjustHeight(double height);
    }
}
