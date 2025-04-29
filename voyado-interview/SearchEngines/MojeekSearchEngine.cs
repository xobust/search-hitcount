
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using interview.Models;

namespace interview.SearchEngines;

/// <summary>
/// Implementation using https://www.mojeek.com search api 
/// https://www.mojeek.com/support/api/search/
/// </summary>
public class MojeekSearchEngine(HttpClient httpClient, IConfiguration configuration) : HttpClient, ISearchEngine
{
    public string Name => "Mojeek";

    private readonly string _apiKey = configuration["MojeekSearchEngine:API_KEY"]
        ?? throw new Exception("Mojeek api key not set");

    private record ResponseEnvelope(QueryResponse Response);

    private record QueryResponse(string Status, ResponseHead Head);

    private record ResponseHead(string Status, SearchWord[] Words);

    private record SearchWord(string Full, ulong Hits);


    public async Task<IEnumerable<KeyWordHitCount>> WordHitCounts(IEnumerable<string> searchWords, CancellationToken cancellation)
    {
        string query = string.Join(" ", searchWords);
        string requestUri = $"?q={Uri.EscapeDataString(query)}&api_key={_apiKey}&fmt=json";

        // The Mojeek API response is quiet large here I show how we can
        // Utilize a stream to optimize the memory usage
        // see: https://josef.codes/you-are-probably-still-using-httpclient-wrong-and-it-is-destabilizing-your-software/
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var result = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation).ConfigureAwait(false);
        result.EnsureSuccessStatusCode();

        // Deserialize lower case json properties
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        //using var contentStream = await result.Content.ReadAsStreamAsync();
        //QueryResponse? responseData = await JsonSerializer.DeserializeAsync<QueryResponse>(contentStream, cancellationToken: cancellation);


        string content = await result.Content.ReadAsStringAsync(cancellation);
        ResponseEnvelope? responseData = JsonSerializer.Deserialize<ResponseEnvelope>(content, jsonOptions);

        if (responseData?.Response.Status != "OK")
        {
            throw new Exception($"Search response status: {responseData?.Response.Status}");
        }

        ResponseHead head = responseData.Response.Head;
        if (head.Words.Length == 0)
        {
            throw new Exception("No words found in search response");
        }

        return head.Words.Select(word => new KeyWordHitCount(word.Full, word.Hits));
    }
}
