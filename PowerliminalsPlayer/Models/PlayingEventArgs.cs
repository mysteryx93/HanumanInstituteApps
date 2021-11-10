using System;

namespace HanumanInstitute.PowerliminalsPlayer.Models
{
    /// <summary>
    /// Contains playback event data.
    /// </summary>
    public class PlayingEventArgs : EventArgs
    {
        public string FileName { get; set; } = string.Empty;

        public PlayingEventArgs()
        { }

        public PlayingEventArgs(string fileName)
        {
            this.FileName = fileName;
        }
    }
}
