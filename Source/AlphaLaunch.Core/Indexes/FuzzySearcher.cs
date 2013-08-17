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

        public void IndexItems(FileItem[] items)
        {
            _searchIndex.AppendToIndex(items);
        }

        public ImmutableList<SearchResult> Search(string search)
        {
            return _fuzzyMatcher.Find(search)
                .Select(x => new SearchResult(x.MatchedString, x.Score, x.TargetItem, x.MatchedIndexes))
                .ToImmutableList();
        }
    }
}