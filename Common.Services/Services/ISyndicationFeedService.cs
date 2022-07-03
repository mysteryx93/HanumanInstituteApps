using System.ServiceModel.Syndication;

namespace HanumanInstitute.Common.Services;

/// <summary>
/// Provides access to RSS feeds.
/// </summary>
public interface ISyndicationFeedService
{
    /// <summary>
    /// Loads a RSS feed from specified URI.
    /// </summary>
    /// <param name="uri">The URI to load.</param>
    /// <returns>A SyndicationFeed object.</returns>
    SyndicationFeed Load(string uri);

    /// <summary>
    /// Loads a RSS feed from specified URI.
    /// </summary>
    /// <param name="uri">The URI to load.</param>
    /// <returns>A SyndicationFeed object.</returns>
    SyndicationFeed Load(Uri uri);
}
