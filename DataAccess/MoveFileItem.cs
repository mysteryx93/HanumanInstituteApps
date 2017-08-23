using System;
using System.ComponentModel;

namespace DataAccess {
    public class MoveFileItem : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid VideoId { get; set; }
        public MediaType MediaType { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public Guid? MediaCategoryId { get; set; }
        public string FileName { get; set; }
        public string NewFileName { get; set; }
        public bool FileExists { get; set; }
    }
}
