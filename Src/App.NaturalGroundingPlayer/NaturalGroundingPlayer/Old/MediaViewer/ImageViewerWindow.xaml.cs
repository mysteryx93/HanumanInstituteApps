using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for ImageViewerWindow.xaml
    /// </summary>
    public partial class ImageViewerWindow : Window {

        #region Declaration / Constructor 

        public static ImageViewerWindow Instance(UIElement imageElement) {
            ImageViewerWindow NewForm = new ImageViewerWindow();
            NewForm.ImgGrid.Children.Insert(0, imageElement);
            NewForm.AdjustImage();
            NewForm.Show();
            return NewForm;
        }

        private WindowResizer ob;
        private double scale = 1;
        private double offsetX = 0;
        private double offsetY = 0;
        private bool isRightButtonPressed;
        private DispatcherTimer timerHideResizer;
        public double resizerOpacity;
        public static readonly DependencyProperty ResizerOpacityProperty = DependencyProperty.Register("ResizerOpacity", typeof(double), typeof(ImageViewerWindow));

        public double ResizerOpacity {
            get { return (double)this.GetValue(ResizerOpacityProperty); }
            set { this.SetValue(ResizerOpacityProperty, value); }
        }

        public ImageViewerWindow() {
            InitializeComponent();
            ob = new WindowResizer(this);
            timerHideResizer = new DispatcherTimer();
            timerHideResizer.Interval = TimeSpan.FromSeconds(1);
            timerHideResizer.Tick += timerHideResizer_Tick;
        }

        #endregion

        #region Adjust Image
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            AdjustImage();
        }

        private void AdjustImage() {
            ImgContentCtrl.Width = ImgCanvas.ActualWidth * scale;
            Canvas.SetLeft(ImgContentCtrl, ImgCanvas.ActualWidth * (1 - scale) / 2 + offsetX);
            ImgContentCtrl.Height = ImgCanvas.ActualHeight * scale;
            Canvas.SetTop(ImgContentCtrl, ImgCanvas.ActualHeight * (1 - scale) / 2 + offsetY);
        }

        #endregion

        #region Mouse Events

        private void Resize(object sender, MouseButtonEventArgs e) {
            timerHideResizer.Stop();
            ob.resizeWindow(sender);
        }

        private void DisplayResizeCursor(object sender, MouseEventArgs e) {
            timerHideResizer.Stop();
            ob.displayResizeCursor(sender);
        }

        private void ResetCursor(object sender, MouseEventArgs e) {
            timerHideResizer.Start();
            ob.resetCursor();
        }

        private void timerHideResizer_Tick(object sender, EventArgs e) {
            timerHideResizer.Stop();
            ResizerOpacity = 0;
        }

        private void ImgThumb_DragDelta(object sender, DragDeltaEventArgs e) {
            if (isRightButtonPressed) {
                offsetX += e.HorizontalChange;
                offsetY += e.VerticalChange;
                AdjustImage();
            } else {
                this.Left += e.HorizontalChange;
                this.Top += e.VerticalChange;
            }
        }

        private void ImgThumb_MouseWheel(object sender, MouseWheelEventArgs e) {
            // Zoom in when the user scrolls the mouse wheel up and vice versa.
            if (e.Delta > 0) {
                // Limit zoom-in to 500%
                if (scale < 5)
                    scale += 0.1;
            } else {
                // When mouse wheel is scrolled down...
                // Limit zoom-out to 80%
                if (scale > 0.8)
                    scale -= 0.1;
            }
            AdjustImage();
        }

        private void ImgThumb_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.RightButton == MouseButtonState.Pressed)
                isRightButtonPressed = true;
        }

        private void ImgThumb_MouseUp(object sender, MouseButtonEventArgs e) {
            if (e.RightButton == MouseButtonState.Released)
                isRightButtonPressed = false;
        }

        private void ImgThumb_MouseMove(object sender, MouseEventArgs e) {
            ResizerOpacity = 1;
            timerHideResizer.Stop();
            timerHideResizer.Start();
        }

        #endregion

    }
}