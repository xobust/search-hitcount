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
            foreach (var searchWord in searchWords)
            {
                var hitCount = await searchEngine.GetHitCount(searchWord, cancellation);
                searchEngineResults.Add(new SearchEngineResult(searchEngine.Name,
                    new KeyWordHitCount(searchWord, hitCount)));
            }
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

