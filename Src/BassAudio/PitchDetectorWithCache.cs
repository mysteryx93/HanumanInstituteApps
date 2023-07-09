using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace HanumanInstitute.BassAudio;

/// <summary>
/// Provides audio pitch-detection. The data is cached for 1 day.
/// </summary>
public class PitchDetectorWithCache : PitchDetector, IPitchDetectorWithCache
{
    private readonly IAppCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromDays(1);
    private CancellationTokenSource _cacheToken = new();
    private const string CachePrefix = "BassAudio.PitchDetector: ";

    /// <summary>
    /// Initializes a new instance of the PitchDetector class.
    /// </summary>
    public PitchDetectorWithCache(IFileSystemService fileSystem, IAppCache cache) : base(fileSystem)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    protected override void PassesChanged()
    {
        _cacheToken.Cancel();
        _cacheToken = new CancellationTokenSource();
        base.PassesChanged();
    }

    /// <inheritdoc />
    public override Task<float> GetPitchAsync(string filePath) =>
        _cache.GetOrAddAsync(CachePrefix + filePath,
            () => base.GetPitchAsync(filePath), new MemoryCacheEntryOptions()
            {
                SlidingExpiration = _cacheExpiration,
                ExpirationTokens = { new CancellationChangeToken(_cacheToken.Token) }
            });

    /// <inheritdoc />
    public override float GetPitch(string filePath) =>
        _cache.GetOrAdd(CachePrefix + filePath,
            () => base.GetPitch(filePath), _cacheExpiration);
}
