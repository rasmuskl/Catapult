using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaLaunch.Experiments
{
    public class SearchIndex
    {
        public SearchIndex(IEnumerable<string> inputStrings)
        {
            Entries = inputStrings
                .Select(x => new IndexEntry(x))
                .ToArray();
        }

        public IndexEntry[] Entries { get; private set; }
    }
}