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
    public partial class LayerVideoControl : UserControl, ILayer {
        public static double ItemHeight = 46;
        private WmpPlayerBusiness playerBusiness;
        private WmpPlayerWindow playerWindow;
        public event EventHandler Closing;
#pragma warning disable CS0414
        private bool isClosed;

        public Media Item { get; private set; }

        public LayerVideoControl() {
            InitializeComponent();
        }

        public async Task OpenMediaAsync(Media item) {
            this.Item = item;

            TitleText.Text = Item.Title;
            TitleText.ToolTip = Item.Title;
            PositionText.Text = "";
            playerWindow = WmpPlayerWindow.Instance();
            playerBusiness = new WmpPlayerBusiness(new WmpPlayerController(playerWindow.Player, playerWindow));
            playerBusiness.AllowClose = true;
            playerBusiness.Closed += playerBusiness_Closed;
            playerBusiness.PositionChanged += playerBusiness_PositionChanged;
            playerBusiness.Show();
            await playerBusiness.PlayVideoAsync(Item, false);
        }

        private void playerBusiness_Closed(object sender, EventArgs e) {
            isClosed = true;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void LoopButton_Click(object sender, RoutedEventArgs e) {
            playerBusiness.player.Loop = LoopButton.IsChecked.Value;
        }

        private void playerBusiness_PositionChanged(object sender, EventArgs e) {
            Business.MediaTimeConverter Conv = new Business.MediaTimeConverter();
            PositionText.Text = string.Format("{0} / {1}",
                Conv.Convert(playerBusiness.Position, typeof(string), null, null),
                Conv.Convert(playerBusiness.CurrentVideo.Length, typeof(string), null, null));
        }

        public void Close() {
            playerBusiness.Close();
            //if (!isClosed)
            //    playerWindow.Close();
            if (Closing != null)
                Closing(this, new EventArgs());
        }

        public void Hide() {
            playerWindow.Visibility = System.Windows.Visibility.Hidden;
        }

        public void Show() {
            playerWindow.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
