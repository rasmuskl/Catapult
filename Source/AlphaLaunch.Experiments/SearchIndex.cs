using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Experiments
{
    public class SearchIndex
    {
        public SearchIndex(IEnumerable<string> inputStrings = null)
        {
            Entries = ImmutableList.Create<IndexEntry>();
            AppendToIndex(inputStrings);
        }

        public void AppendToIndex(IEnumerable<string> inputStrings)
        {
            inputStrings = inputStrings ?? Enumerable.Empty<string>();
            Entries = Entries.AddRange(inputStrings.Select(x => new IndexEntry(x)));
        }

        public ImmutableList<IndexEntry> Entries { get; private set; }
    }
}