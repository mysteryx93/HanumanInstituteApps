using System.Net;
using System.Net.Http;
using LazyCache;

namespace HanumanInstitute.Common.Services;

/// <inheritdoc />
public class UpdateService : IUpdateService
{
    private readonly ISyndicationFeedService _feedService;
    private readonly HttpClient _httpClient;
    private readonly IAppCache _cache;
    private bool _init;
    private const string CacheKey = "UpdateService.GetLatestVersion";

    public UpdateService(ISyndicationFeedService feedService, HttpClient httpClient, IAppCache cache)
    {
        _feedService = feedService;
        _httpClient = httpClient;
        _cache = cache;
    }

    /// <inheritdoc />
    public string GitRepo { get; set; } = "https://github.com/mysteryx93/NaturalGroundingPlayer";

    /// <inheritdoc />
    public string GetDownloadInfoLink() => GitRepo + "/releases";

    /// <inheritdoc />
    public string FileFormat { get; set; } = string.Empty;

    /// <inheritdoc />
    public string FeedUrl => GitRepo + "/releases.atom";

    /// <inheritdoc />
    public Task<Version?> GetLatestVersionAsync() => 
        Task.Run(GetLatestVersion);

    /// <inheritdoc />
    public Version? GetLatestVersion() =>
        _cache.GetOrAdd(CacheKey, GetLatestVersionImpl, TimeSpan.FromHours(1));
    
    private Version? GetLatestVersionImpl()
    {
        if (!FileFormat.HasValue())
        {
            throw new ArgumentException("FileFormat and GitRepo must be set before calling GetLatestVersion.", nameof(FileFormat));
        }
        if (!_init)
        {
            _httpClient.BaseAddress = new Uri(GitRepo);
            _init = true;
        }

        try
        {
            var feed = _feedService.Load(FeedUrl);
            foreach (var item in feed.Items)
            {
                // Id looks like this. Get version number. 
                // tag:github.com,2008:Repository/37950127/v2.0.1
                var pos = item.Id.LastIndexOf('/');
                if (pos > -1)
                {
                    var version = item.Id[(pos + 2)..];

                    // Produce download string like this.
                    // https://github.com/mysteryx93/NaturalGroundingPlayer/releases/download/v2.0.1/Converter432Hz-2.0.1_Win_x64.zip
                    var downloadLink = GitRepo + string.Format("/releases/download/v{0}/" + FileFormat, version);

                    // Check if link exists.
                    if (UrlExists(downloadLink))
                    {
                        return Version.Parse(version);
                    }
                }
            }
        }
        catch (HttpRequestException)
        {
        }

        return null;
    }

    /// <summary>
    /// Returns whether specified URI exists.
    /// </summary>
    /// <param name="uri">The uri to verify.</param>
    /// <returns>True if uri is valid, otherwise false.</returns>
    private bool UrlExists(string uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, new Uri(uri));
        try
        {
            var response = _httpClient.Send(request);
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }
}
