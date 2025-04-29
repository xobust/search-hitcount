

using System.Text.Json;

namespace interview.SearchEngines;

/// <summary>
/// Search engine utilizing googles custom search engine API
/// https://developers.google.com/custom-search/v1/overview
/// </summary>
public class GoogleCustomSearchEngine(IConfiguration configuration, HttpClient httpClient): HttpClient, ISearchEngine
{
    private readonly string apiKey = configuration["GoogleCustomSearchEngine:API_KEY"]
        ?? throw new Exception("secret GoogleCustomSearchEngine:API_KEY");
    private readonly string searchEngineId = configuration["GoogleCustomSearchEngine:SEARCH_ENGINE_ID"]
        ?? throw new Exception("secret GoogleCustomSearchEngine:SEARCH_ENGINE_ID not set");

    private record QueryResponse(Queries Queries);
    private record Queries(List<Request> Request);
    private record Request(string TotalResults);

    public string Name => "Google Custom Search Engine";

    public async Task<ulong> GetHitCount(string searchWord, CancellationToken cancellation)
    {
        string requestUri = $"?key={apiKey}&cx={searchEngineId}&q={Uri.EscapeDataString(searchWord)}" +
            $"&alt=json&fields=queries(request(totalResults))";

        HttpResponseMessage result = await httpClient.GetAsync(requestUri, cancellation);

        result.EnsureSuccessStatusCode();

        string content = await result.Content.ReadAsStringAsync(cancellation);

        // Deserialize lower case json properties
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        QueryResponse responseData = JsonSerializer.Deserialize<QueryResponse>(content, jsonOptions)
            ?? throw new Exception("Failied to deserialize search response");

        if (responseData.Queries.Request.Count == 0)
        {
            throw new Exception("No request found in search response");
        }

        ulong totalResults = ulong.Parse(responseData.Queries.Request[0].TotalResults);
        return totalResults;
    }
}
