using System;
using System.Drawing;
using System.Windows;
using HanumanInstitute.CommonWpf;

namespace HanumanInstitute.CommonWpfApp {
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window {
        public static SplashWindow Instance(Window owner, Bitmap image) {
            SplashWindow F = new SplashWindow();
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
