using interview.SearchEngines;
using interview.Models;

namespace interview;

public partial class SearchAgregator(IEnumerable<ISearchEngine> searchEngines) : ISearchAgregator
{

    public async Task<AgregateSearchResult> GetAgregateSearchResult(string searchQuery, CancellationToken cancellation)
    {
        char[] whitespace = [' ', '\t', '\n', '\r'];
        string[] searchWords = searchQuery.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);

        var searchEngineResults = new List<SearchEngineResult>();
        foreach (var searchEngine in searchEngines)
        {
            var hitCount = await searchEngine.WordHitCounts(searchWords, cancellation);
            hitCount.Select(hitCount => new SearchEngineResult(
                searchEngine.Name,
                hitCount
            )).ToList().ForEach(searchEngineResults.Add);
        }

        var totalHits = searchEngineResults
            .GroupBy(x => x.KeyWordHitCount.KeyWord)
            .Select(g => new KeyWordHitCount(
                g.Key,
                g.Aggregate(0UL, (acc, x) => acc + x.KeyWordHitCount.HitCount)
            )).ToArray();
        return new AgregateSearchResult(searchEngineResults.ToArray(), totalHits);
    }
}

