using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using System.Xml.Serialization;

namespace Business {
    public class SyncBusiness {
        public string GetSampleXml() {
            using (Entities context = new Entities()) {
                var MediaData = (from v in context.Media
                                 where v.Title.StartsWith("Ukraine Got Talent") || v.Title.StartsWith("Electric Boy")
                              select v).Take(2).ToList();
                var MediaSyncData = (from v in MediaData select new MediaSync(v)).ToList();
                string Xml = XmlHelper.Serialize<List<MediaSync>>(MediaSyncData, "MediaList");
                return Xml;
            }
        }
    }
}
