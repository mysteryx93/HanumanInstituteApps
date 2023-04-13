using System.Text.Json.Serialization;
using HanumanInstitute.Apps.AdRotator;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

[JsonSerializable(typeof(AppSettingsData))]
[JsonSerializable(typeof(PlayingItem))]
[JsonSerializable(typeof(AdInfo))]
[JsonSerializable(typeof(HanumanInstituteHttpClient.AppVersionQuery))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
