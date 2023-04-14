using System.Text.Json.Serialization;
using HanumanInstitute.Apps.AdRotator;

namespace HanumanInstitute.YangDownloader.Business;

[JsonSerializable(typeof(AppSettingsData))]
[JsonSerializable(typeof(AdInfo))]
[JsonSerializable(typeof(AppVersionQuery))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
