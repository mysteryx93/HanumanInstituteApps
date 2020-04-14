using System;
using System.Collections.Generic;
using System.Linq;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    /// <summary>
    /// Manages the playback of a list of media files.
    /// </summary>
    public interface IAudioPlayerManager
    {
        /// <summary>
        /// Returns the list of files currently playing.
        /// </summary>
        List<string> Files { get; }
        /// <summary>
        /// Returns the path of the file currently playing.
        /// </summary>
        string NowPlaying { get; set; }
        /// <summary>
        /// Occurs when a media file starts to play.
        /// </summary>
        event EventHandler<PlayingEventArgs> StartPlaying;
        /// <summary>
        /// Starts the playback of specified list of media files.
        /// </summary>
        /// <param name="list">The list of file paths to play.</param>
        /// <param name="current">If specified, playback will start with specified file.</param>
        void Play(IEnumerable<string> list, string current);
        /// <summary>
        /// Starts playing the next media file from the list.
        /// </summary>
        void PlayNext();
    }

    /// <summary>
    /// Manages the playback of a list of media files.
    /// </summary>
    public class AudioPlayerManager : IAudioPlayerManager
    {
        /// <summary>
        /// Returns the list of files currently playing.
        /// </summary>
        public List<string> Files { get; private set; }
        /// <summary>
        /// Returns the path of the file currently playing.
        /// </summary>
        public string NowPlaying { get; set; }
        /// <summary>
        /// Occurs when a media file starts to play.
        /// </summary>
        public event EventHandler<PlayingEventArgs> StartPlaying;
        private Random random = new Random();

        private IAppPathService appPath;
        private IFileSystemService fileSystem;

        public AudioPlayerManager(IAppPathService appPathService, IFileSystemService fileSystemService)
        {
            this.appPath = appPathService ?? throw new ArgumentNullException(nameof(appPathService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        /// <summary>
        /// Starts the playback of specified list of media files.
        /// </summary>
        /// <param name="list">The list of file paths to play.</param>
        /// <param name="current">If specified, playback will start with specified file.</param>
        public void Play(IEnumerable<string> list, string current)
        {
            Files = list.ToList();
            NowPlaying = current;
            if (current != null)
                StartPlaying?.Invoke(this, new PlayingEventArgs(NowPlaying));
            else
                PlayNext();
        }

        /// <summary>
        /// Starts playing the next media file from the list.
        /// </summary>
        public void PlayNext()
        {
            if (Files != null && Files.Count() > 0)
            {
                int Pos = random.Next(Files.Count);
                if (Files[Pos] == NowPlaying)
                    Pos = random.Next(Files.Count);
                NowPlaying = Files[Pos];
                StartPlaying?.Invoke(this, new PlayingEventArgs(NowPlaying));
            }
        }
    }
}
