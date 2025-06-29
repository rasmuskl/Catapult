using System.Collections.Immutable;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class StringSearchFrame(Func<string, SearchResult[]>? autocompleterFunc) : ISearchFrame
{
    public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
    {
        var results = new[] { new SearchResult(search, 0, new StringIndexable(search), ImmutableHashSet.Create<int>())  };

        if (autocompleterFunc == null)
        {
            return results;
        }

        SearchResult[] autocompleteResults = autocompleterFunc(search);
        return results.Concat(autocompleteResults).ToArray();
    }
}