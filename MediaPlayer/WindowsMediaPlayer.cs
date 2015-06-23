using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace MediaPlayer {
    public partial class WindowsMediaPlayer : UserControl {
        
        public event EventHandler MediaOpened;
        public event EventHandler MediaResume;
        public event EventHandler MediaPause;
        public event EventHandler PositionChanged;

        public WindowsMediaPlayer() {
            InitializeComponent();

            // Editor doesn't keep player settings.
            Player.settings.volume = 100;
            Player.uiMode = "full";
            Player.enableContextMenu = false;
            Player.stretchToFit = true;
        }

        public string Source {
            get {
                return Player.URL;
            }
            set {
                Player.URL = value;
            }
        }

        public bool IsPlaying {
            get { return Player.playState == WMPLib.WMPPlayState.wmppsPlaying; }
        }

        public void Play() {
            Player.Ctlcontrols.play();
        }

        public void Pause() {
            Player.Ctlcontrols.pause();
        }

        public void Stop() {
            Player.Ctlcontrols.stop();
        }

        public void SetFramePosition(double pos) {
            Player.Ctlcontrols.pause();
            Player.Ctlcontrols.currentPosition = pos;
            ((IWMPControls2)Player.Ctlcontrols).step(1);
        }

        public double Position {
            get {
                return Player.Ctlcontrols.currentPosition;
            }
            set {
                Player.Ctlcontrols.currentPosition = value;
            }
        }

        public double Duration {
            get {
                return Player.currentMedia.duration;
            }
        }

        public int VideoWidth {
            get {
                return Player.currentMedia.imageSourceWidth;
            }
        }

        public int VideoHeight {
            get {
                return Player.currentMedia.imageSourceHeight;
            }
        }

        public bool Loop {
            get {
                return Player.settings.getMode("loop");
            }
            set {
                Player.settings.setMode("loop", value);
            }
        }

        public int Volume {
            get {
                return Player.settings.volume;
            }
            set {
                Player.settings.volume = value;
            }
        }

        public bool FullScreen {
            get {
                return Player.fullScreen;
            }
            set {
                Player.fullScreen = value;
            }
        }

        private void Player_OpenStateChange(object sender, AxWMPLib._WMPOCXEvents_OpenStateChangeEvent e) {
            if (e.newState == (int)WMPLib.WMPOpenState.wmposMediaOpen) {
                if (MediaOpened != null)
                    MediaOpened(this, new EventArgs());
            }
        }

        private void Player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e) {
            if (e.newState == (int)WMPLib.WMPPlayState.wmppsPlaying) {
                if (MediaResume != null)
                    MediaResume(this, new EventArgs());
            } else if (e.newState == (int)WMPLib.WMPPlayState.wmppsPaused || e.newState == (int)WMPLib.WMPPlayState.wmppsStopped) {
                if (MediaPause != null)
                    MediaPause(this, new EventArgs());
            }
        }

        private void Player_PositionChange(object sender, AxWMPLib._WMPOCXEvents_PositionChangeEvent e) {
            if (PositionChanged != null)
                PositionChanged(this, new EventArgs());
        }
    }
}
