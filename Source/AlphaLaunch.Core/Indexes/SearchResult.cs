using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.Core.Indexes
{
    public class SearchResult
    {
        public string Name { get; set; }
        public double Score { get; set; }
        public string FullPath { get; set; }
        public ImmutableDictionary<int, double> HighlightIndexes { get; set; }
    }
}