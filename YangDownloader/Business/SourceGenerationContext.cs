using System.Text.Json.Serialization;

namespace HanumanInstitute.YangDownloader.Business;

[JsonSerializable(typeof(AppSettingsData))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
