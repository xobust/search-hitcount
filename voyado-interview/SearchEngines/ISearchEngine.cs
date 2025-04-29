
namespace interview.SearchEngines;


public interface ISearchEngine
{
    string Name { get; }
    public Task<ulong> GetHitCount(string searchWord, CancellationToken cancellation);
}