using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.Core.Indexes
{
    public class SearchResult
    {
        public SearchResult(string name, double score, object targetItem, ImmutableDictionary<int, double> highlightIndexes)
        {
            Name = name;
            Score = score;
            TargetItem = targetItem;
            HighlightIndexes = highlightIndexes;
        }

        public string Name { get; private set; }
        public double Score { get; private set; }
        public object TargetItem { get; private set; }
        public ImmutableDictionary<int, double> HighlightIndexes { get; private set; }
    }
}