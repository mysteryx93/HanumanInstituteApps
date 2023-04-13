using System.ServiceModel.Syndication;
using System.Xml;

namespace HanumanInstitute.Services;

/// <summary>
/// Provides access to RSS feeds.
/// </summary>
public class SyndicationFeedService : ISyndicationFeedService
{
    /// <summary>
    /// Loads a RSS feed from specified URI.
    /// </summary>
    /// <param name="uri">The URI to load.</param>
    /// <returns>A SyndicationFeed object.</returns>
    public SyndicationFeed Load(string uri) => Load(new Uri(uri));

    /// <summary>
    /// Loads a RSS feed from specified URI.
    /// </summary>
    /// <param name="uri">The URI to load.</param>
    /// <returns>A SyndicationFeed object.</returns>
    public SyndicationFeed Load(Uri uri)
    {
        uri.CheckNotNull(nameof(uri));

        using var reader = XmlReader.Create(uri.AbsoluteUri);
        return SyndicationFeed.Load(reader);
    }
}
