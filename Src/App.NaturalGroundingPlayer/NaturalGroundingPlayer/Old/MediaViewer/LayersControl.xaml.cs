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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Business;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for LayersControl.xaml
    /// </summary>
    public partial class LayersControl : UserControl {
        public LayersControl() {
            InitializeComponent();
        }

        public void Add(UserControl layer) {
            LayersList.Children.Add(layer);
            ((ILayer)layer).Closing += layer_Closing;
            this.Height += layer.Height;

            ((ILayerContainer)Window.GetWindow(this)).AdjustHeight(layer.Height);
        }

        void layer_Closing(object sender, EventArgs e) {
            Remove(sender as UserControl);
        }

        public void Remove(UserControl layer) {
            LayersList.Children.Remove(layer);
            this.Height -= layer.ActualHeight;

            ((ILayerContainer)Window.GetWindow(this)).AdjustHeight(-layer.Height);
        }

        private void SetMarginBottom(FrameworkElement element, double bottomChange) {
            element.Margin = new Thickness(element.Margin.Left, element.Margin.Top, element.Margin.Right, element.Margin.Bottom + bottomChange);
        }

        public void RemoveAll() {
            while (LayersList.Children.Count > 0) {
                ((ILayer)LayersList.Children[0]).Close();
            }
        }
    }
}
