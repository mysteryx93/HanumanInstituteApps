using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HanumanInstitute.Services;

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
    public Task SerializeAsync<T>(Stream utf8Json, T dataToSerialize, IJsonTypeInfoResolver? serializerContext) =>
        JsonSerializer.SerializeAsync(utf8Json, dataToSerialize, typeof(T), GetOptions(serializerContext));

    /// <inheritdoc />
    public T Deserialize<T>(string data, IJsonTypeInfoResolver? serializerContext)
        where T : class, new() =>
        (T)JsonSerializer.Deserialize(data, typeof(T), GetOptions(serializerContext))!;

    /// <inheritdoc />
    public async ValueTask<T> DeserializeAsync<T>(Stream utf8Json, IJsonTypeInfoResolver? serializerContext)
        where T : class, new() =>
        (T)await JsonSerializer.DeserializeAsync(utf8Json, typeof(T), GetOptions(serializerContext)).ConfigureAwait(false)!;

    /// <inheritdoc />
    public void SerializeToFile<T>(T dataToSerialize, string path, IJsonTypeInfoResolver? serializerContext)
    {
        _fileSystem.EnsureDirectoryExists(path);
        using var writer = _fileSystem.FileStream.New(path, FileMode.Create);
        JsonSerializer.Serialize(writer, dataToSerialize, typeof(T), GetOptions(serializerContext));
        writer.Flush();
    }
    
    /// <inheritdoc />
    public async Task SerializeToFileAsync<T>(T dataToSerialize, string path, IJsonTypeInfoResolver? serializerContext)
    {
        _fileSystem.EnsureDirectoryExists(path);
        await using var writer = _fileSystem.FileStream.New(path, FileMode.Create);
        await JsonSerializer.SerializeAsync(writer, dataToSerialize, typeof(T), GetOptions(serializerContext));
        writer.Flush();
    }
    
    /// <inheritdoc />
    public T DeserializeFromFile<T>(string path, IJsonTypeInfoResolver? serializerContext)
    {
        var stream = _fileSystem.File.OpenRead(path);
        return (T)JsonSerializer.Deserialize(stream, typeof(T), GetOptions(serializerContext))!;
    }
    
    /// <inheritdoc />
    public async ValueTask<T> DeserializeFromFileAsync<T>(string path, IJsonTypeInfoResolver? serializerContext)
    {
        var stream = _fileSystem.File.OpenRead(path);
        return (T)(await JsonSerializer.DeserializeAsync(stream, typeof(T), GetOptions(serializerContext)))!;
    }
}
