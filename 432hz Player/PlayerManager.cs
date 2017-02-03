using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player432hz {
    public class PlayerManager {
        public List<string> Files { get; internal set; }
        public string NowPlaying { get; set; }
        public event EventHandler<PlayingEventArgs> StartPlaying;
        private Random random = new Random();

        public PlayerManager() {
        }

        public void Play(IEnumerable<string> list, string current) {
            Files = list.ToList();
            NowPlaying = current;
            StartPlaying?.Invoke(this, new PlayingEventArgs(NowPlaying));
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

        public static IEnumerable<string> GetFiles(string path,
                       string[] searchPatterns,
                       SearchOption searchOption = SearchOption.TopDirectoryOnly) {
            return searchPatterns.AsParallel()
                   .SelectMany(searchPattern =>
                          Directory.EnumerateFiles(path, searchPattern, searchOption));
        }
    }

    public class PlayingEventArgs {
        public string FileName { get; set; }

        public PlayingEventArgs(string fileName) {
            this.FileName = fileName;
        }
    }
}
