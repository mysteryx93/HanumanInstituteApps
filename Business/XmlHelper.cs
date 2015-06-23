using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Business {
    public class XmlHelper {
        public static string Serialize<T>(T dataToSerialize, string root) {
            var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(root));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(stringwriter, dataToSerialize, ns);
            return stringwriter.ToString();
        }

        public static T Deserialize<T>(string xmlText) {
            var stringReader = new StringReader(xmlText);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stringReader);
        }
    }
}
