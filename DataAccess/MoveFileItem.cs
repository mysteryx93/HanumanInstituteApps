using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    [PropertyChanged.ImplementPropertyChanged]
    public class MoveFileItem {
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
