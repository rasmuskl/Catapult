using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Core.Indexes
{
    public class FuzzySearcher : ISearcher
    {
        private readonly FuzzyMatcher _fuzzyMatcher;
        private readonly SearchIndex _searchIndex;

        public FuzzySearcher()
        {
            _searchIndex = new SearchIndex();
            _fuzzyMatcher = new FuzzyMatcher(_searchIndex);
        }

        public void IndexItems(IEnumerable<IIndexable> items)
        {
            _searchIndex.AppendToIndex(items);
        }

        public ImmutableList<SearchResult> Search(string search, ImmutableDictionary<string, EntryBoost> boostEntries)
        {
            return _fuzzyMatcher.Find(search, boostEntries)
                .Select(x => new SearchResult(x.MatchedString, x.Score, x.TargetItem, x.MatchedIndexes))
                .ToImmutableList();
        }
    }
}