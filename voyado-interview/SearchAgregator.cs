using interview.SearchEngines;
using interview.Models;
using System.Diagnostics;

namespace interview;

/// <summary>
/// Collects results for all availible ISerachEngines and returns a combined result
/// </summary>
public class SearchAgregator(IEnumerable<ISearchEngine> searchEngines, ILogger<SearchAgregator> logger) : ISearchAgregator
{
    private static readonly ActivitySource activitySource = new(nameof(SearchAgregator)); 

    public async Task<AgregateSearchResult> GetAgregateSearchResult(string searchQuery, CancellationToken cancellation)
    {
        char[] whitespace = [' ', '\t', '\n', '\r'];
        string[] searchWords = searchQuery.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);

        var searchEngineResults = new List<SearchEngineResult>();
        foreach (var searchEngine in searchEngines)
        {
            using Activity? acivity = activitySource.CreateActivity($"QueryEngine.{searchEngine.Name}", ActivityKind.Internal);
            try
            {
                var hitCount = await searchEngine.WordHitCounts(searchWords, cancellation);
                hitCount.Select(hitCount => new SearchEngineResult(
                    searchEngine.Name,
                    hitCount
                )).ToList().ForEach(searchEngineResults.Add);
            } catch (Exception ex)
            {
                logger.LogError(ex, "Failed to query engine {}", searchEngine.Name);
            }
        }

        // Sum the total hits
        var totalHits = searchEngineResults
            .GroupBy(searchEngineResult => searchEngineResult.KeyWordHitCount.KeyWord)
            .Select(g => new KeyWordHitCount(
                g.Key,
                g.Aggregate(0UL, (acc, result) => acc + result.KeyWordHitCount.HitCount)
            )).ToArray();

        return new AgregateSearchResult(searchEngineResults.ToArray(), totalHits);
    }
}

