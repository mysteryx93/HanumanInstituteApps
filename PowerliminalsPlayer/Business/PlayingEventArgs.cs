using System;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{

    /// <summary>
    /// Contains playback event data.
    /// </summary>
    public class PlayingEventArgs : EventArgs
    {
        public string FileName { get; set; }

        public PlayingEventArgs(string fileName)
        {
            this.FileName = fileName;
        }
    }
}
