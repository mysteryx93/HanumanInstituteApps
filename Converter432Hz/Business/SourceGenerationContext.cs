using System.Text.Json.Serialization;

namespace HanumanInstitute.Converter432Hz.Business;

[JsonSerializable(typeof(AppSettingsData))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
