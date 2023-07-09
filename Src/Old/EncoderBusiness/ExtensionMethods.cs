using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.MediaEncoder {
    internal static class ExtensionMethods {
        /// <summary>
        /// Creates a deep copy of specified object so that all references are new.
        /// </summary>
        /// <param name="obj">The object to duplicate.</param>
        /// <returns>A new object with separate references.</returns>
        public static object DeepClone(this object obj) {
            object objResult = null;
            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);

                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }

        /// <summary>
        /// Copies all fields from one instance of a class to another.
        /// </summary>
        /// <typeparam name="T">The type of class to copy.</typeparam>
        /// <param name="source">The class to copy.</param>
        /// <param name="target">The class to copy to.</param>
        public static void CopyAll<T>(T source, T target) {
            var type = typeof(T);
            foreach (var sourceProperty in type.GetProperties()) {
                var targetProperty = type.GetProperty(sourceProperty.Name);
                if (targetProperty.SetMethod != null)
                    targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
            }
            foreach (var sourceField in type.GetFields()) {
                var targetField = type.GetField(sourceField.Name);
                targetField.SetValue(target, sourceField.GetValue(source));
            }
        }
    }
}
