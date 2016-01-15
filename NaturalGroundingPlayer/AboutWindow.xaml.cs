using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window {
        public static void Instance() {
            AboutWindow NewForm = new AboutWindow();
            SessionCore.Instance.Windows.ShowDialog(NewForm);
        }

        private WindowHelper helper;

        public AboutWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
            // VersionText.Text = SessionCore.GetVersionText();
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void LicenseLink_Click(object sender, RoutedEventArgs e) {
            LicenseWindow.Instance();
        }
    }
}
