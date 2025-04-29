namespace interview.Models;

public record AgregateSearchResult(SearchEngineResult[] SearchEngineResults, KeyWordHitCount[] TotalHits);

public record KeyWordHitCount(string KeyWord, ulong HitCount);

public record SearchEngineResult(string Name, KeyWordHitCount KeyWordHitCount);

