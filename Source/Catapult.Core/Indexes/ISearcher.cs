using System.Collections.Immutable;

namespace Catapult.Core.Indexes;

public interface ISearcher
{
    void IndexItems(IEnumerable<IIndexable> items);
    ImmutableList<SearchResult> Search(string search, ImmutableDictionary<string, EntryBoost> boostEntries);
}