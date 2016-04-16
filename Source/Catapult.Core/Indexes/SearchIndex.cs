using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Catapult.Core.Indexes
{
    public class SearchIndex
    {
        public SearchIndex(IEnumerable<IIndexable> inputObjects = null)
        {
            Entries = ImmutableList.Create<IndexEntry>();
            AppendToIndex(inputObjects);
        }

        public void AppendToIndex(IEnumerable<IIndexable> inputStrings)
        {
            inputStrings = inputStrings ?? Enumerable.Empty<IIndexable>();
            Entries = Entries.AddRange(inputStrings.Select(x => new IndexEntry(x.Name, x)));
        }

        public ImmutableList<IndexEntry> Entries { get; private set; }
    }
}