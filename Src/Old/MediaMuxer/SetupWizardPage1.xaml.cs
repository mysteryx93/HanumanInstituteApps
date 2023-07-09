using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HanumanInstitute.MediaMuxer {
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SetupWizardPage1 : Page, IWizardPage {
        public SetupWizardPage1() {
            InitializeComponent();
        }

        public MainWindow Owner { get; set; }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            Owner.NextButton.IsEnabled = false;
        }

        private void MuxeButton_Click(object sender, RoutedEventArgs e) {
            Owner.CurrentPage = 10;
        }

        private void MergeButton_Click(object sender, RoutedEventArgs e) {
            Owner.CurrentPage = 20;
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e) {
            Owner.CurrentPage = 30;
        }

        public bool Validate() {
            return true;
        }
    }
}
