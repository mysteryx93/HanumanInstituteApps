using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Business;
using DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for LayerControl.xaml
    /// </summary>
    public partial class LayerImageControl : ILayer {
        public static double ItemHeight = 46;
        private ImageViewerWindow viewer;
        public event EventHandler Closing;
        private bool isClosed;
        public Media Item { get; private set; }

        public LayerImageControl() {
            InitializeComponent();
        }

        public void OpenMedia(Media item) {
            this.Item = item;

            BitmapImage NewImage = new BitmapImage();
            NewImage.BeginInit();
            NewImage.UriSource = new Uri(Settings.NaturalGroundingFolder + item.FileName);
            NewImage.EndInit();
            Image NewImageControl = new Image();
            NewImageControl.Source = NewImage;

            TitleText.Text = Item.Title;
            TitleText.ToolTip = Item.Title;

            viewer = ImageViewerWindow.Instance(NewImageControl);
            viewer.Closed += viewer_Closed;
        }

        private void viewer_Closed(object sender, EventArgs e) {
            if (!isClosed) {
                isClosed = true;
                Close();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (viewer != null)
                viewer.Opacity = e.NewValue;
        }

        public void Close() {
            if (!isClosed) {
                isClosed = true;
                viewer.Close();
            }
            if (Closing != null)
                Closing(this, new EventArgs());
        }


        public void Hide() {
            viewer.Visibility = System.Windows.Visibility.Hidden;
        }

        public void Show() {
            viewer.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
