using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Business;

namespace PowerliminalsPlayer {
    /// <summary>
    /// Interaction logic for PlayerInstance.xaml
    /// </summary>
    public partial class PlayerInstance : UserControl {
        public PlayerInstance() {
            InitializeComponent();
            WmpPlayer.Player.MediaStop += Player_MediaStop;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            if (!App.HasExited) {
                WmpPlayer.Player.Stop();
                WmpPlayer.Player.Dispose();
            }
        }

        private void Player_MediaStop(object sender, EventArgs e) {
            if (IsLoaded) {
                var files = Host.config.Current.Files;
                FileItem Item = files.FirstOrDefault(f => f.Id == Id);
                if (Item != null)
                    files.Remove(Item);
            }
        }

        // Allows binding a unique ID to identify this instance in the list.
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(Guid),
            typeof(PlayerInstance));

        public Guid Id {
            get {
                return (Guid)GetValue(IdProperty);
            }
            set {
                SetValue(IdProperty, value);
            }
        }

        private MainWindow Host {
            get {
                var P = Window.GetWindow(this) as MainWindow;
                if (P == null)
                    throw new Exception("PlayerInstance can only be placed on MainWindow");
                return P;
            }
        }
    }
}
