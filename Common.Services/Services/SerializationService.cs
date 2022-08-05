using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MessagePack;

// ReSharper disable CheckNamespace
namespace HanumanInstitute.Common.Services;

/// <inheritdoc />
public class SerializationService : ISerializationService
{
    private readonly IFileSystemService _fileSystem;
    
    public SerializationService(IFileSystemService fileSystemService)
    {
        _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public string Serialize<T>(T dataToSerialize)
    {
        using var stringWriter = new StringWriter();
        var serializer = new XmlSerializer(typeof(T));
        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty);
        serializer.Serialize(stringWriter, dataToSerialize, ns);
        return stringWriter.ToString();
    }

    /// <inheritdoc />
    public T Deserialize<T>(string xmlText) where T : class, new()
    {
        using var stringReader = new StringReader(xmlText);
        using var xmlReader = XmlReader.Create(stringReader,
            new XmlReaderSettings() { XmlResolver = null });
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(xmlReader)!;
    }

    /// <inheritdoc />
    public T DeserializeFromFile<T>(string path)
    {
        var stream = _fileSystem.File.OpenRead(path);
        using var reader = XmlReader.Create(stream,
            new XmlReaderSettings() { XmlResolver = null });
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(reader)!;
    }

    /// <inheritdoc />
    public void SerializeToFile<T>(T dataToSerialize, string path)
    {
        _fileSystem.EnsureDirectoryExists(path);
        using var writer = _fileSystem.FileStream.Create(path, FileMode.Create);
        var serializer = new XmlSerializer(typeof(T));
        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty);
        serializer.Serialize(writer, dataToSerialize, ns);
        writer.Flush();
    }
}
