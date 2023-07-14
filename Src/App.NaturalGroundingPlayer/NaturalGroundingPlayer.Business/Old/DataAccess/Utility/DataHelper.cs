using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public class DataHelper {
        /// <summary>
        /// Creates a deep copy of specified object so that all references are new.
        /// </summary>
        /// <param name="obj">The object to duplicate.</param>
        /// <returns>A new object with separate references.</returns>
        //public static object DeepClone(object obj) {
        //    object objResult = null;
        //    using (MemoryStream ms = new MemoryStream()) {
        //        BinaryFormatter bf = new BinaryFormatter();
        //        bf.Serialize(ms, obj);

        //        ms.Position = 0;
        //        objResult = bf.Deserialize(ms);
        //    }
        //    return objResult;
        //}
    }
}
