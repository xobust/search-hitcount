using System.Text.Json;
using System.Text.Json.Serialization;
using interview.Models;

namespace interview.SearchEngines;

/// <summary>
/// Implementation using https://www.searchapi.io
/// Can be used with multiple sources that supports total_results
/// </summary>
/// <param name="httpClient"></param>
/// <param name="engine"></param>
public class SearchApiIoEngine(HttpClient httpClient, IConfiguration configuration, string engine) : HttpClient, ISearchEngine
{
    public string Name => engine;

    private readonly string _apiKey = configuration["SearchApiIoEngine:API_KEY"]
        ?? throw new Exception("Search.io api key not set");

    private record SearchInformation(ulong? TotalResults);

    private record Response(SearchInformation SearchInformation);

    public Task<IEnumerable<KeyWordHitCount>> WordHitCounts(IEnumerable<string> searchWords, CancellationToken cancellation)
    {
        var tasks = searchWords.Select(searchWord => GetHitCount(searchWord, cancellation));
        return Task.WhenAll(tasks).ContinueWith(t =>
        {
            return searchWords.Zip(t.Result, (word, count) => new KeyWordHitCount(word, count));
        }, cancellation);
    }

    public async Task<ulong> GetHitCount(string searchWord, CancellationToken cancellation)
    {
        string queryString = $"?q={Uri.EscapeDataString(searchWord)}&engine={engine}&api_key={_apiKey}";
        var result = await httpClient.GetAsync(queryString, cancellation);

        result.EnsureSuccessStatusCode();

        string content = await result.Content.ReadAsStringAsync(cancellation);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        Response responseData = JsonSerializer.Deserialize<Response>(content, options)
            ?? throw new Exception("Invalid reponse data");

        return responseData.SearchInformation.TotalResults ?? 0;
    }
}
