using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for InitializingWindow.xaml
    /// </summary>
    public partial class InitializingWindow2 : Window {
        private WindowHelper helper;

        public InitializingWindow2() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }
    }
}
