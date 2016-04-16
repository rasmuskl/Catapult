using System;
using System.Diagnostics;
using System.Linq;
using Catapult.Core.Indexes;
using Serilog;

namespace Catapult.Core.Selecta
{
    public class Searcher
    {
        private readonly IIndexable[] _allItems;
        private readonly IIndexable[] _matchedItems;
        private readonly SelectaSearcher _selecta;

        public string SearchString { get; }
        public SearchResult[] SearchResults { get; }

        public static Searcher Create(IIndexable[] allItems)
        {
            return new Searcher(allItems, allItems, string.Empty, new SearchResult[0]);
        }

        private Searcher(IIndexable[] allItems, IIndexable[] matchedItems, string searchString, SearchResult[] searchResults)
        {
            _allItems = allItems;
            _matchedItems = matchedItems;
            SearchString = searchString;
            SearchResults = searchResults;

            _selecta = new SelectaSearcher();
        }

        public Searcher Search(string searchString, Func<IIndexable, int> boosterFunc = null)
        {
            searchString = searchString ?? string.Empty;
            boosterFunc = boosterFunc ?? (x => 0);

            if (string.IsNullOrEmpty(searchString))
            {
                return Create(_allItems);
            }

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

            var searchResults = matches
                .OrderBy(x => x.MatchScore.Score - x.Boost)
                .ThenBy(x => x.Indexable.Name.Length)
                .ThenBy(x => x.MatchScore.Range.EndIndex - x.MatchScore.Range.StartIndex)
                .Select(x => new SearchResult(x.Indexable.Name, x.MatchScore.Score - x.Boost, x.Indexable, x.MatchScore.MatchSet))
                .ToArray();

            var matchedItems = matches.Select(x => x.Indexable).ToArray();

            scoreStopwatch.Stop();

            Log.Information($"Found {matches.Length} results of {items.Length} for {searchString} [ scr: {scoreStopwatch.ElapsedMilliseconds} ms ]");

            return new Searcher(_allItems, matchedItems, searchString, searchResults);
        }
    }
}