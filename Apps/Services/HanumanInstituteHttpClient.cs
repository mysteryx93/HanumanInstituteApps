using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HanumanInstitute.Apps.AdRotator;
using LazyCache;

namespace HanumanInstitute.Apps;

/// <inheritdoc cref="IHanumanInstituteHttpClient" />
public class HanumanInstituteHttpClient : HttpClient, IHanumanInstituteHttpClient
{
    private readonly IAppInfo _appInfo;
    private readonly ISerializationService _serializationService;
    private readonly IJsonTypeInfoResolver _serializerContext;
    private readonly IEnvironmentService _environment;
    private readonly IAppCache _cache;

    private const string BaseUrl = "https://store.spiritualselftransformation.com/api/";
    private const string QueryVersionUrl = BaseUrl + "app-version?app={0}&os={1}";
    private const string GetAdsUrl = BaseUrl + "app-ads";
    private const string QueryCacheKey = "HanumanInstituteHttpClient.QueryVersionAsync";
    private const string LinkTrackerUrl = BaseUrl + "click?app={0}&os={1}&ad={2}";

    /// <summary>
    /// Initializes a new instance of the HanumanInstituteHttpClient class.
    /// </summary>
    public HanumanInstituteHttpClient(IAppInfo appInfo, ISerializationService serializationService, IJsonTypeInfoResolver serializerContext, IEnvironmentService environment, IAppCache cache)
    {
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
                var stream = await GetStreamAsync(url);
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
            var stream = await GetStreamAsync(GetAdsUrl);
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

    /// <summary>
    /// Version information returned by HanumanInstituteHttpClient.QueryVersion.
    /// </summary>
    public class AppVersionQuery
    {
        /// <summary>
        /// The latest app version available for download.
        /// </summary>
        public Version LatestVersion { get; set; } = default!;
        /// <summary>
        /// The date ads were last updated on the server, in UTC time.
        /// </summary>
        public DateTime AdsLastUpdated { get; set; }
    }
}
