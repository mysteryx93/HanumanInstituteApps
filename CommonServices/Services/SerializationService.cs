using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Manages the serialization of objects.
    /// </summary>
    public class SerializationService : ISerializationService
    {
        private readonly IFileSystemService _fileSystem;

        //public SerializationService() : this(new FileSystemService()) { }

        public SerializationService(IFileSystemService fileSystemService)
        {
            _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        /// <summary>
        /// Serializes an object of specified type to a string.
        /// </summary>
        /// <typeparam name="T">The data type of the object to serialize.</typeparam>
        /// <param name="dataToSerialize">The object to serialize.</param>
        /// <param name="root">The root namespace for the generated XML.</param>
        /// <returns>An XML string containing serialized data.</returns>
        public string Serialize<T>(T dataToSerialize)
        {
            using var stringWriter = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            serializer.Serialize(stringWriter, dataToSerialize, ns);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Deserializes an object of specified type from a string.
        /// </summary>
        /// <typeparam name="T">The data type of the object to deserialize.</typeparam>
        /// <param name="xmlText">The XML string containing the data to deserialize.</param>
        /// <param name="root">The root namespace of the XML.</param>
        /// <exception cref="InvalidOperationException">An error occurred during deserialization. The original exception is available using the InnerException property.</exception>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string xmlText) where T : class, new()
        {
            using var stringReader = new StringReader(xmlText);
            using var xmlReader = XmlReader.Create(stringReader,
                new XmlReaderSettings() { XmlResolver = null });
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// Loads an object of specified type from an XML file.
        /// </summary>
        /// <typeparam name="T">The data type of the object to serialize.</typeparam>
        /// <param name="path">The path of the file from which to read XML data.</param>
        /// <exception cref="InvalidOperationException">An error occurred during deserialization. The original exception is available using the InnerException property.</exception>
        /// <returns>The object created from the file<./returns>
        public T DeserializeFromFile<T>(string path)
        {
            using var reader = XmlReader.Create(path,
                new XmlReaderSettings() { XmlResolver = null });
            var serializer = new XmlSerializer(typeof(T));
            var result = (T)serializer.Deserialize(reader);
            return result;
        }

        /// <summary>
        /// Saves an object to an xml file.
        /// </summary>
        /// <typeparam name="T">The data type of the object to serialize.</typeparam>
        /// <param name="dataToSerialize">The object to serialize.</param>
        /// <param name="path">The path of the file in which to output the XML data.</param>
        public void SerializeToFile<T>(T dataToSerialize, string path)
        {
            _fileSystem.EnsureDirectoryExists(path);
            using var writer = _fileSystem.FileStream.Create(path, System.IO.FileMode.Create);
            var serializer = new XmlSerializer(typeof(T));
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            serializer.Serialize(writer, dataToSerialize, ns);
            writer.Flush();
        }
    }
}
