using System.Text.Json.Serialization;
using HanumanInstitute.Apps.AdRotator;

namespace HanumanInstitute.Converter432Hz.Business;

[JsonSerializable(typeof(AppSettingsData))]
[JsonSerializable(typeof(AdInfo))]
[JsonSerializable(typeof(HanumanInstituteHttpClient.AppVersionQuery))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
