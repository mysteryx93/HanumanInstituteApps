using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;

namespace AudioVideoMuxer {
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
