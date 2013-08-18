using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App
{
    public class SearchItemModel
    {
        public string Name { get; set; }
        public double Score { get; set; }
        public object TargetItem { get; set; }
        public Guid Id { get; set; }
        public ImmutableDictionary<int, double> HighlightIndexes { get; set; }

        public SearchItemModel(string name, double score, object targetItem, ImmutableDictionary<int, double> highlightIndexes)
        {
            Name = name;
            Score = score;
            TargetItem = targetItem;
            HighlightIndexes = highlightIndexes;
        }
    }
}