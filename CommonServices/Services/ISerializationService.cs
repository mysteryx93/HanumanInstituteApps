using System;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Manages the serialization of objects.
    /// </summary>
    public interface ISerializationService
    {
        /// <summary>
        /// Serializes an object of specified type to a string.
        /// </summary>
        /// <typeparam name="T">The data type of the object to serialize.</typeparam>
        /// <param name="dataToSerialize">The object to serialize.</param>
        /// <param name="root">The root namespace for the generated XML.</param>
        /// <returns>An XML string containing serialized data.</returns>
        string Serialize<T>(T dataToSerialize);
        /// <summary>
        /// Deserializes an object of specified type from a string.
        /// </summary>
        /// <typeparam name="T">The data type of the object to deserialize.</typeparam>
        /// <param name="xmlText">The XML string containing the data to deserialize.</param>
        /// <param name="root">The root namespace of the XML.</param>
        /// <exception cref="InvalidOperationException">An error occurred during deserialization. The original exception is available using the InnerException property.</exception>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(string xmlText) where T : class, new();
        /// <summary>
        /// Loads an object of specified type from an XML file.
        /// </summary>
        /// <typeparam name="T">The data type of the object to serialize.</typeparam>
        /// <param name="path">The path of the file from which to read XML data.</param>
        /// <exception cref="InvalidOperationException">An error occurred during deserialization. The original exception is available using the InnerException property.</exception>
        /// <returns>The object created from the file<./returns>
        T DeserializeFromFile<T>(string path);
        /// <summary>
        /// Saves an object to an xml file.
        /// </summary>
        /// <typeparam name="T">The data type of the object to serialize.</typeparam>
        /// <param name="dataToSerialize">The object to serialize.</param>
        /// <param name="path">The path of the file in which to output the XML data.</param>
        void SerializeToFile<T>(T dataToSerialize, string path);
    }
}
