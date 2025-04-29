using Autofac;
using interview.SearchEngines;
using IContainer = Autofac.IContainer;

namespace interview.Tests;

/// <summary>
/// Mock implementation of ISearchEngine for testing purposes.
/// </summary>
public class TestSearchEngine : ISearchEngine
{
    public string Name { get; }
    private readonly Dictionary<string, ulong> _hitCounts;
    private readonly bool _simulateCancellation;

    public TestSearchEngine(string name, Dictionary<string, ulong> hitCounts, bool simulateCancellation = false)
    {
        Name = name;
        _hitCounts = hitCounts;
        _simulateCancellation = simulateCancellation;
    }

    public Task<ulong> GetHitCount(string searchWord, CancellationToken cancellation)
    {
        if (_simulateCancellation && cancellation.IsCancellationRequested)
        {
            throw new TaskCanceledException();
        }

        _hitCounts.TryGetValue(searchWord, out var hitCount);
        return Task.FromResult(hitCount);
    }
}

public class SearchAggregatorTests
{
    private IContainer BuildContainer(IEnumerable<ISearchEngine> searchEngines)
    {
        var builder = new ContainerBuilder();
        foreach (var searchEngine in searchEngines)
        {
            builder.RegisterInstance(searchEngine).As<ISearchEngine>();
        }
        return builder.Build();
    }

    [Fact]
    public async Task GetAgregateSearchResult_ReturnsCorrectAggregateResult()
    {
        // Arrange
        var mockSearchEngine1 = new TestSearchEngine("Engine1", new Dictionary<string, ulong>
        {
            { "test", 10 }
        });

        var mockSearchEngine2 = new TestSearchEngine("Engine2", new Dictionary<string, ulong>
        {
            { "test", 20 }
        });

        var container = BuildContainer(new[] { mockSearchEngine1, mockSearchEngine2 });
        var searchEngines = container.Resolve<IEnumerable<ISearchEngine>>();
        var searchAgregator = new SearchAgregator(searchEngines);

        string searchQuery = "test";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await searchAgregator.GetAgregateSearchResult(searchQuery, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.SearchEngineResults.Length);
        Assert.Single(result.TotalHits);
        Assert.Equal("test", result.TotalHits[0].KeyWord);
        Assert.Equal((ulong)30, result.TotalHits[0].HitCount);
    }

    [Fact]
    public async Task GetAgregateSearchResult_HandlesEmptySearchQuery()
    {
        // Arrange
        var mockSearchEngine = new TestSearchEngine("Engine1", new Dictionary<string, ulong>());
        var container = BuildContainer(new[] { mockSearchEngine });
        var searchEngines = container.Resolve<IEnumerable<ISearchEngine>>();
        var searchAgregator = new SearchAgregator(searchEngines);

        string searchQuery = "";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await searchAgregator.GetAgregateSearchResult(searchQuery, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.SearchEngineResults);
        Assert.Empty(result.TotalHits);
    }

    [Fact]
    public async Task GetAgregateSearchResult_HandlesMultipleWordsInQuery()
    {
        // Arrange
        var mockSearchEngine = new TestSearchEngine("Engine1", new Dictionary<string, ulong>
        {
            { "word1", 5 },
            { "word2", 15 }
        });

        var container = BuildContainer(new[] { mockSearchEngine });
        var searchEngines = container.Resolve<IEnumerable<ISearchEngine>>();
        var searchAgregator = new SearchAgregator(searchEngines);

        string searchQuery = "word1 word2";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await searchAgregator.GetAgregateSearchResult(searchQuery, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.SearchEngineResults.Length);
        Assert.Equal(2, result.TotalHits.Length);
        Assert.Contains(result.TotalHits, x => x.KeyWord == "word1" && x.HitCount == 5);
        Assert.Contains(result.TotalHits, x => x.KeyWord == "word2" && x.HitCount == 15);
    }

    [Fact]
    public async Task GetAgregateSearchResult_CancelsOperation()
    {
        // Arrange
        var mockSearchEngine = new TestSearchEngine("Engine1", new Dictionary<string, ulong>(), simulateCancellation: true);
        var container = BuildContainer(new[] { mockSearchEngine });
        var searchEngines = container.Resolve<IEnumerable<ISearchEngine>>();
        var searchAgregator = new SearchAgregator(searchEngines);

        string searchQuery = "test";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            searchAgregator.GetAgregateSearchResult(searchQuery, cancellationToken));
    }
}
