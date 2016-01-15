using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow : Window {
        public static void Instance() {
            LicenseWindow NewForm = new LicenseWindow();
            SessionCore.Instance.Windows.ShowDialog(NewForm);
        }

        private WindowHelper helper;

        public LicenseWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
            try {
                LicenseText.Text = File.ReadAllText("License.txt");
            } catch {
                LicenseText.Text = "License.txt not found.";
            }
            LicenseText.Select(0, 0);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
