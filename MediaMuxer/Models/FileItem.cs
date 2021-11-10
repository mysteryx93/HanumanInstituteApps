using System;

namespace HanumanInstitute.MediaMuxer.Models
{
    public class FileItem
    {
        public string Path { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;

        public FileItem()
        {
        }

        public FileItem(string path, string display)
        {
            Path = path;
            Display = display;
        }
    }
}
