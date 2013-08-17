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
        public string FullPath { get; set; }
        public Guid Id { get; set; }
        public ImmutableDictionary<int, double> HighlightIndexes { get; set; }

        public SearchItemModel(string name, double score, string fullPath, ImmutableDictionary<int, double> highlightIndexes)
        {
            Name = name;
            Score = score;
            FullPath = fullPath;
            HighlightIndexes = highlightIndexes;
        }

        public string DisplayName
        {
            get { return string.Format("{0} ({1})", Name, Score); }
        }
    }
}