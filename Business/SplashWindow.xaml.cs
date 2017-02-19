using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

namespace Business {
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window {
        public static SplashWindow Instance(Window owner, Bitmap image) {
            SplashWindow F = new Business.SplashWindow();
            F.Owner = owner;
            F.ImageBox.Source = image.ToBitmapImage();
                // new BitmapImage(new Uri(string.Format("/{0};component/{1}", app, file), UriKind.RelativeOrAbsolute));
            F.Width = F.ImageBox.Source.Width;
            F.Height = F.ImageBox.Source.Height;
            // F.Show();
            return F;
        }

        private bool canClose = false;

        public SplashWindow() {
            InitializeComponent();
        }

        private async void CloseMethod(object sender, EventArgs e) {
            while (!canClose) {
                await System.Threading.Tasks.Task.Delay(100);
            }

            this.Topmost = false;
            this.Close();
            this.window.WindowState = WindowState.Normal;
            this.Owner.Activate();
        }

        public void CanClose() {
            canClose = true;
        }
    }
}
