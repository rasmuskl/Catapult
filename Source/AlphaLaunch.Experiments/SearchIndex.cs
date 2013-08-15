using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Experiments
{
    public class SearchIndex
    {
        public SearchIndex(IEnumerable<string> inputStrings)
        {
            Entries = ImmutableList.From(inputStrings.Select(x => new IndexEntry(x)));
        }

        public ImmutableList<IndexEntry> Entries { get; private set; }
    }
}