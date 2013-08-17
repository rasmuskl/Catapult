using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AlphaLaunch.Experiments;

namespace AlphaLaunch.Core.Indexes
{
    public class FuzzySearcher : ISearcher
    {
        private readonly FuzzyMatcher _fuzzyMatcher;
        private readonly SearchIndex _searchIndex;

        public FuzzySearcher()
        {
            _searchIndex = new SearchIndex(new string[0]);
            _fuzzyMatcher = new FuzzyMatcher(_searchIndex);
        }

        public void IndexItems(FileItem[] items)
        {
            _searchIndex.AppendToIndex(items.Select(x => x.Name));
        }

        public ImmutableList<SearchResult> Search(string search)
        {
            return _fuzzyMatcher.Find(search)
                .Select(x => new SearchResult
                {
                    Name = x.MatchedString,
                    Score = x.Score,
                })
                .ToImmutableList();
        }
    }
}