using System.Text.Json.Serialization;

namespace HanumanInstitute.Player432Hz.Business;

[JsonSerializable(typeof(AppSettingsData))]
[JsonSerializable(typeof(List<SettingsPlaylistItem>))]
[JsonSerializable(typeof(List<string>))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
