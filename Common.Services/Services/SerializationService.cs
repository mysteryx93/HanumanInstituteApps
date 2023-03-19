using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

// ReSharper disable CheckNamespace
namespace HanumanInstitute.Common.Services;

/// <inheritdoc />
public class SerializationService : ISerializationService
{
    private readonly IFileSystemService _fileSystem;
    
    public SerializationService(IFileSystemService fileSystemService)
    {
        _fileSystem = fileSystemService;
    }

    private JsonSerializerOptions GetOptions(IJsonTypeInfoResolver? serializerContext) =>
        new() {
            TypeInfoResolver = serializerContext,
            IgnoreReadOnlyProperties = true,
            WriteIndented = true
        };

    /// <inheritdoc />
    public string Serialize<T>(T dataToSerialize, IJsonTypeInfoResolver? serializerContext) =>
        JsonSerializer.Serialize(dataToSerialize, typeof(T), GetOptions(serializerContext));

    /// <inheritdoc />
    public T Deserialize<T>(string data, IJsonTypeInfoResolver? serializerContext)
        where T : class, new() =>
        (T)JsonSerializer.Deserialize(data, typeof(T), GetOptions(serializerContext))!;

    /// <inheritdoc />
    public T DeserializeFromFile<T>(string path, IJsonTypeInfoResolver? serializerContext)
    {
        var stream = _fileSystem.File.OpenRead(path);
        return (T)JsonSerializer.Deserialize(stream, typeof(T), GetOptions(serializerContext))!;
    }

    /// <inheritdoc />
    public void SerializeToFile<T>(T dataToSerialize, string path, IJsonTypeInfoResolver? serializerContext)
    {
        _fileSystem.EnsureDirectoryExists(path);
        using var writer = _fileSystem.FileStream.New(path, FileMode.Create);
        JsonSerializer.Serialize(writer, dataToSerialize, typeof(T), GetOptions(serializerContext));
        writer.Flush();
    }
}
