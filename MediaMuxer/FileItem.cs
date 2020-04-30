using System;

namespace EmergenceGuardian.AudioVideoMuxer {
    public class FileItem {
        public string Path { get; set; }
        public string Display { get; set; }

        public FileItem() {
        }

        public FileItem( string path, string display) {
            Path = path;
            Display = display;
        }
    }
}
