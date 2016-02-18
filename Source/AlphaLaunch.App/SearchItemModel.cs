using System.Collections.Immutable;
using System;
using System.Drawing;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.App
{
    public class SearchItemModel
    {
        public string Name { get; set; }
        public double Score { get; set; }
        public IIndexable TargetItem { get; set; }
        public Guid Id { get; set; }
        public ImmutableDictionary<int, double> HighlightIndexes { get; set; }
        public IIconResolver IconResolver { get; set; }

        public SearchItemModel(string name, double score, IIndexable targetItem, ImmutableDictionary<int, double> highlightIndexes, IIconResolver iconResolver)
        {
            Name = name;
            Score = score;
            TargetItem = targetItem;
            HighlightIndexes = highlightIndexes;
            IconResolver = iconResolver;
        }
    }
}