
using interview.Models;

namespace interview.SearchEngines;

public interface ISearchEngine
{
    /// <summary>
    /// Name of the search engine
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Queries search engine for the given search words and returns the number of hits
    /// </summary>
    /// <returns>The number of hits for each word in the query if any</returns>
    public Task<IEnumerable<KeyWordHitCount>> WordHitCounts(IEnumerable<string> searchWords, CancellationToken cancellation);
}