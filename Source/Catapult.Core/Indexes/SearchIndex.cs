using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Catapult.Core.Indexes
{
    public class SearchIndex
    {
        public SearchIndex()
        {
            Entries = ImmutableDictionary.Create<string, ImmutableList<IndexEntry>>();
        }

        public ImmutableDictionary<string, ImmutableList<IndexEntry>> Entries { get; }

        public void AppendToIndex(string key, IEnumerable<IIndexable> items)
        {
            items = items ?? Enumerable.Empty<IIndexable>();
            Entries.SetItem(key, ImmutableList.Create(items.Select(x => new IndexEntry(x.Name, x)).ToArray()));
        }
    }
}