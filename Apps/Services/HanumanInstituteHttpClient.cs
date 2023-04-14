using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HanumanInstitute.Apps.AdRotator;
using LazyCache;

namespace HanumanInstitute.Apps;

/// <inheritdoc cref="IHanumanInstituteHttpClient" />
public class HanumanInstituteHttpClient : IHanumanInstituteHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IAppInfo _appInfo;
    private readonly ISerializationService _serializationService;
    private readonly IJsonTypeInfoResolver _serializerContext;
    private readonly IEnvironmentService _environment;
    private readonly IAppCache _cache;

    private const string BaseUrl = "https://store.spiritualselftransformation.com/api/";
    private const string QueryVersionUrl = BaseUrl + "app-version?app={0}&os={1}";
    private const string GetAdsUrl = BaseUrl + "app-ads";
    private const string LinkTrackerUrl = BaseUrl + "app-click?app={0}&os={1}&ad={2}";
    private const string QueryCacheKey = "HanumanInstituteHttpClient.QueryVersionAsync";

    /// <summary>
    /// Initializes a new instance of the HanumanInstituteHttpClient class.
    /// </summary>
    public HanumanInstituteHttpClient(HttpClient httpClient, IAppInfo appInfo, ISerializationService serializationService, 
        IJsonTypeInfoResolver serializerContext, IEnvironmentService environment, IAppCache cache)
    {
        _httpClient = httpClient;
        _appInfo = appInfo;
        _serializationService = serializationService;
        _serializerContext = serializerContext;
        _environment = environment;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<AppVersionQuery?> QueryVersionAsync()
    {
        try
        {
            return await _cache.GetOrAddAsync(QueryCacheKey, async () =>
            {
                var url = QueryVersionUrl.FormatInvariant((int)_appInfo.Id, _environment.GetRuntimeIdentifier());
                var stream = await _httpClient.GetStreamAsync(url);
                return await _serializationService.DeserializeAsync<AppVersionQuery>(stream, _serializerContext);
            }, TimeSpan.FromHours(1));
        }
        catch (HttpRequestException) { }
        catch (TaskCanceledException) { }
        catch (JsonException) { }
        return null;
    }

    /// <inheritdoc />
    public async Task<AdInfo?> GetAdsAsync()
    {
        try
        {
            var stream = await _httpClient.GetStreamAsync(GetAdsUrl);
            return await _serializationService.DeserializeAsync<AdInfo>(stream, _serializerContext);
        }
        catch (HttpRequestException) { }
        catch (TaskCanceledException) { }
        catch (JsonException) { }
        return null;
    }

    /// <inheritdoc />
    public string GetLinkTrackerUrl(int ad) =>
        LinkTrackerUrl.FormatInvariant((int)_appInfo.Id, _environment.GetRuntimeIdentifier(), ad);
}
