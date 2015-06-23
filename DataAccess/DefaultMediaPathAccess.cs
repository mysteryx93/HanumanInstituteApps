using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public static class DefaultMediaPathAccess {
        public static List<ArtistMediaCount> GetMediaCountPerArtist() {
            using (Entities context = new Entities()) {
                var Result = (from m in context.Media
                              where m.Artist != ""
                              group m.MediaId by new { m.MediaTypeId, m.Artist } into a
                              select new ArtistMediaCount() { 
                                  MediaType = (MediaType)a.Key.MediaTypeId, 
                                  Artist = a.Key.Artist, 
                                  Count = a.Count()
                              }).ToList();
                return Result;
            }
        }

        public static List<MediaCategory> GetCategories() {
            using (Entities context = new Entities()) {
                var Result = (from c in context.MediaCategories
                              select c).ToList();
                return Result;
            }
        }
    }

    public class ArtistMediaCount {
        public MediaType MediaType { get; set; }
        public string Artist { get; set; }
        public int Count;

        public ArtistMediaCount() {
        }

        public ArtistMediaCount(MediaType mediaType, string artist, int count) {
            this.MediaType = mediaType;
            this.Artist = artist;
            this.Count = count;
        }
    }
}
