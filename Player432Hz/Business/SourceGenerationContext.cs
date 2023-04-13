using System.Text.Json.Serialization;
using HanumanInstitute.Apps.AdRotator;

namespace HanumanInstitute.Player432Hz.Business;

[JsonSerializable(typeof(AppSettingsData))]
[JsonSerializable(typeof(List<SettingsPlaylistItem>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(AdInfo))]
[JsonSerializable(typeof(HanumanInstituteHttpClient.AppVersionQuery))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
