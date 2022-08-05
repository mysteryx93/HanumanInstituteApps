using System.IO;
using MessagePack;

namespace HanumanInstitute.Common.Services;

public class CloneExtension
{
    public byte[] Serialize<T>(T thisObj)
    {
        using var byteStream = new MemoryStream();
        MessagePackSerializer.Serialize(byteStream, thisObj);
        return byteStream.ToArray();
    }

    public T Deserialize<T>(byte[] bytes)
    {
        using var byteStream = new MemoryStream(bytes);
        return MessagePackSerializer.Deserialize<T>(byteStream);
    }
}
