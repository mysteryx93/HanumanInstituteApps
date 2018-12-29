using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    public class AudioPlayerManager {
        public List<string> Files { get; internal set; }
        public string NowPlaying { get; set; }
        public event EventHandler<PlayingEventArgs> StartPlaying;
        private Random random = new Random();

        public AudioPlayerManager() {
        }

        public void Play(IEnumerable<string> list, string current) {
            Files = list.ToList();
            NowPlaying = current;
            if (current != null)
                StartPlaying?.Invoke(this, new PlayingEventArgs(NowPlaying));
            else
                PlayNext();
        }

        public void PlayNext() {
            if (Files != null && Files.Count() > 0) {
                int Pos = random.Next(Files.Count);
                if (Files[Pos] == NowPlaying)
                    Pos = random.Next(Files.Count);
                NowPlaying = Files[Pos];
                StartPlaying?.Invoke(this, new PlayingEventArgs(NowPlaying));
            }
        }

        public static IEnumerable<string> GetAudioFiles(string path) {
            return GetFiles(path, AppPaths.AudioExtensions, SearchOption.AllDirectories);
        }

        private static IEnumerable<string> GetFiles(string path, string[] searchPatterns, SearchOption searchOption = SearchOption.TopDirectoryOnly) {
            try {
                return Directory.EnumerateFiles(path, "*", searchOption).Where(f => searchPatterns.Any(s => f.EndsWith(s)));
            } catch {
                return new string[] { };
            }
        }
    }

    public class PlayingEventArgs {
        public string FileName { get; set; }

        public PlayingEventArgs(string fileName) {
            this.FileName = fileName;
        }
    }
}
