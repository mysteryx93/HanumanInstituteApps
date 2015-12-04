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
using DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for LayerControl.xaml
    /// </summary>
    public partial class LayerAudioControl : ILayer {
        private WmpPlayerBusiness playerBusiness;
        public event EventHandler Closing;
        public Media Item { get; private set; }

        public LayerAudioControl() {
            InitializeComponent();
        }

        public async Task OpenMediaAsync(Media item) {
            this.Item = item;
            TitleText.Text = Item.Title;
            TitleText.ToolTip = Item.Title;
            PositionText.Text = "";
            playerBusiness = new WmpPlayerBusiness(Player);
            playerBusiness.PositionChanged += playerBusiness_PositionChanged;
            playerBusiness.Show();
            await playerBusiness.PlayVideoAsync(Item, false);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void LoopButton_Click(object sender, RoutedEventArgs e) {
            Player.Loop = LoopButton.IsChecked.Value;
        }

        private void playerBusiness_PositionChanged(object sender, EventArgs e) {
            MediaTimeConverter Conv = new MediaTimeConverter();
            PositionText.Text = string.Format("{0} / {1}",
                Conv.Convert(playerBusiness.Position, typeof(string), null, null),
                Conv.Convert(playerBusiness.CurrentVideo.Length, typeof(string), null, null));
        }

        public void Close() {
            playerBusiness.Close();
            if (Closing != null)
                Closing(this, new EventArgs());
        }

        public void Hide() {
        }

        public void Show() {
        }
    }
}
