using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HanumanInstitute.CommonServices;
using PropertyChanged;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Manages the playback of a list of media files.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class PlaylistPlayer : IPlaylistPlayer
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        private readonly IFileSystemService _fileSystem;

        public PlaylistPlayer(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Gets the list of files currently playing.
        /// </summary>
        public IList<string> Files { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the path of the file currently playing.
        /// </summary>
        public string NowPlaying { get; set; }

        /// <summary>
        /// Gets the display title of the file currently playing.
        /// </summary>
        public string NowPlayingTitle { get; set; }

        private readonly Random random = new Random();

        /// <summary>
        /// Starts the playback of specified list of media files.
        /// </summary>
        /// <param name="list">The list of file paths to play.</param>
        /// <param name="current">If specified, playback will start with specified file.</param>
        public void Play(IEnumerable<string> list, string current)
        {
            Files.Clear();
            if (list != null)
            {
                Files.AddRange(list);
            }
            NowPlaying = null;
            NowPlaying = current;
            SetTitle();
            if (string.IsNullOrEmpty(current))
            {
                PlayNext();
            }
        }

        /// <summary>
        /// Starts playing the next media file from the list.
        /// </summary>
        public void PlayNext()
        {
            if (Files.Any())
            {
                int Pos = random.Next(Files.Count);
                if (Files[Pos] == NowPlaying)
                {
                    Pos = random.Next(Files.Count);
                }
                NowPlaying = null;
                NowPlaying = Files[Pos];
                SetTitle();
            }
        }

        private void SetTitle()
        {
            NowPlayingTitle = NowPlaying != null ? _fileSystem.Path.GetFileName(NowPlaying) : null;
        }
    }
}
