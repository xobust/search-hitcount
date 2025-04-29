
using interview.Models;

namespace interview.SearchEngines;

public interface ISearchEngine
{
    string Name { get; }

    public Task<IEnumerable<KeyWordHitCount>> WordHitCounts(IEnumerable<string> searchWords, CancellationToken cancellation);
}