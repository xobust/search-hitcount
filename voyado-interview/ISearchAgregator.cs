
using interview.Models;

namespace interview;

public interface ISearchAgregator
{
    Task<AgregateSearchResult> GetAgregateSearchResult(string searchQuery, CancellationToken cancellation);
}