using System;
using System.Collections.Generic;
using System.Linq;
using AlphaLaunch.Experiments;

namespace AlphaLaunch.Core.Indexes
{
    public interface ISearcher
    {
        void IndexItems(FileItem[] items);
        IEnumerable<SearchResult> Search(string search);
    }

    public class FuzzySearcher : ISearcher
    {
        private FuzzyMatcher _fuzzyMatcher;
        private SearchIndex _searchIndex;

        public FuzzySearcher()
        {
            _searchIndex = new SearchIndex(new string[0]);
            _fuzzyMatcher = new FuzzyMatcher(_searchIndex);
        }

        public void IndexItems(FileItem[] items)
        {
            _searchIndex = new SearchIndex(items.Select(x => x.Name));
            _fuzzyMatcher = new FuzzyMatcher(_searchIndex);
        }

        public IEnumerable<SearchResult> Search(string search)
        {
            return _fuzzyMatcher.Find(search)
                .Select(x => new SearchResult
                {
                    Name = x.MatchedString,
                    Score = x.Score,
                });
        }
    }
}