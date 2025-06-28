using System.Diagnostics;
using Catapult.Core.Indexes;
using Serilog;

namespace Catapult.Core.Selecta;

public class Searcher
{
    private readonly IIndexable[] _allItems;
    private readonly IIndexable[] _matchedItems;
    private readonly SelectaSearcher _selecta;

    public string SearchString { get; }
    public SearchResult[] SearchResults { get; }

    public static Searcher Create(IIndexable[] allItems)
    {
        return new Searcher(allItems, allItems, string.Empty, []);
    }

    private Searcher(IIndexable[] allItems, IIndexable[] matchedItems, string searchString, SearchResult[] searchResults)
    {
        _allItems = allItems;
        _matchedItems = matchedItems;
        SearchString = searchString;
        SearchResults = searchResults;

        _selecta = new SelectaSearcher();
    }

    public Searcher Search(string? searchString, Func<IIndexable, int>? boosterFunc = null)
    {
        searchString ??= string.Empty;
        boosterFunc ??= _ => 0;

        var items = _matchedItems;

        if (!searchString.StartsWith(SearchString))
        {
            items = _allItems;
        }

        var scoreStopwatch = Stopwatch.StartNew();

        var matches = items
            .Select(x => new { MatchScore = _selecta.Score2(searchString, x.Name), Indexable = x, Boost = boosterFunc(x) })
            .Where(x => x.MatchScore.Score < int.MaxValue)
            .ToArray();

        var orderedMatches = matches
            .OrderBy(x => x.MatchScore.Score - x.Boost);

        if (searchString.IsSet())
        {
            orderedMatches = orderedMatches.ThenBy(x => x.Indexable.Name.Length);
        }
            
        orderedMatches = orderedMatches.ThenBy(x => x.MatchScore.Range.EndIndex - x.MatchScore.Range.StartIndex);
            
        var searchResults = orderedMatches.Select(x => new SearchResult(x.Indexable.Name, x.MatchScore.Score - x.Boost, x.Indexable, x.MatchScore.MatchSet)).ToArray();
        var matchedItems = matches.Select(x => x.Indexable).ToArray();

        scoreStopwatch.Stop();

        Log.Information($"Found {matches.Length} results of {items.Length} for {searchString} [ search: {scoreStopwatch.ElapsedMilliseconds} ms ]");

        return new Searcher(_allItems, matchedItems, searchString, searchResults);
    }
}