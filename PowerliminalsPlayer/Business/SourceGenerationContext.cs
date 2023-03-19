using System.Text.Json.Serialization;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

[JsonSerializable(typeof(AppSettingsData))]
[JsonSerializable(typeof(List<PlayingItem>))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
