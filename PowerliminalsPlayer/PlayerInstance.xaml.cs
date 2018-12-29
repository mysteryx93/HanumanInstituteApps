using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Business;
using NAudio.Wave;
using VarispeedDemo.SoundTouch;

namespace PowerliminalsPlayer {
    /// <summary>
    /// Interaction logic for PlayerInstance.xaml
    /// </summary>
    public partial class PlayerInstance : UserControl {
        private IWavePlayer wavePlayer = new WaveOutEvent();
        private LoopStream loopPlayer;
        private VarispeedSampleProvider speedControl;
        private AudioFileReader reader;
        private FileItem binding;

        public PlayerInstance() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            binding = this.DataContext as FileItem;
            LoadFile(binding.FullPath);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            wavePlayer?.Dispose();
            speedControl?.Dispose();
            reader?.Dispose();
        }

        public void Play() {
            wavePlayer.Play();
        }

        public void Pause() {
            wavePlayer.Pause();
        }

        private MainWindow Host {
            get {
                var P = Window.GetWindow(this) as MainWindow;
                if (P == null)
                    throw new Exception("PlayerInstance can only be placed on MainWindow");
                return P;
            }
        }

        private void LoadFile(string file) {
            reader?.Dispose();
            speedControl?.Dispose();
            reader = null;
            speedControl = null;

            if (file == null) return;
            reader = new AudioFileReader(file);
            //DisplayPosition();
            //trackBarPlaybackPosition.Value = 0;
            //trackBarPlaybackPosition.Maximum = (int)(reader.TotalTime.TotalSeconds + 0.5);
            //var useTempo = comboBoxModes.SelectedIndex == 1;
            loopPlayer = new LoopStream(reader);
            speedControl = new VarispeedSampleProvider(loopPlayer.ToSampleProvider(), 100, new SoundTouchProfile(true, false));
            wavePlayer.Init(speedControl);
            wavePlayer.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e) {
            var files = Host.config.Current.Files;
            FileItem Item = files.FirstOrDefault(f => f.Id == binding.Id);
            if (Item != null)
                files.Remove(Item);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            reader.Volume = (float)binding.Volume / 100;
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            speedControl.PlaybackRate = (float)binding.Rate;
        }
    }
}
