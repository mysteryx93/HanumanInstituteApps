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

namespace YinMediaEncoder {
    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class WmpPlayerWindow : Window {
        public static WmpPlayerWindow Instance() {
            WmpPlayerWindow NewForm = new WmpPlayerWindow();
            return NewForm;
        }

        public WmpPlayerWindow() {
            InitializeComponent();
        }
    }
}
