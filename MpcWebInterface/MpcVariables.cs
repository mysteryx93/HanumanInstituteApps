using System;
using System.Collections.Generic;
using System.Text;

namespace EmergenceGuardian.MpcWebInterface {
    /// <summary>
    /// Contains the MPC-HC data returned from Web Interface API¸in 'variables.html'.
    /// </summary>
    public class MpcVariables {
        public MpcVariables() { }

        public string File { get; set; }
        public string FilePathArg { get; set; }
        public string FilePath { get; set; }
        public string FileDirArg { get; set; }
        public string FileDir { get; set; }
        public MpcState State { get; set; }
        public string StateString { get; set; }
        public TimeSpan Position { get; set; }
        public string PositionString { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationString { get; set; }
        public int VolumeLevel { get; set; }
        public bool Muted { get; set; }
        public float PlaybackRate { get; set; }
        public string Size { get; set; }
        public int ReloadTime { get; set; }
        public Version Version { get; set; }
    }

    /// <summary>
    /// Represents the MPC-HC playing states.
    /// </summary>
    public enum MpcState {
        Stopped = 0,
        Paused = 1,
        Playing = 2
    }
}
