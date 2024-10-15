using System.Collections.Immutable;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class StringSearchFrame : ISearchFrame
{
    private readonly Func<string, SearchResult[]>? _autocompleterFunc;

    public StringSearchFrame(Func<string, SearchResult[]>? autocompleterFunc)
    {
        _autocompleterFunc = autocompleterFunc;
    }

    public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
    {
        var results = new[] { new SearchResult(search, 0, new StringIndexable(search), ImmutableHashSet.Create<int>())  };

        if (_autocompleterFunc == null)
        {
            return results;
        }

        SearchResult[] autocompleteResults = _autocompleterFunc(search);
        return results.Concat(autocompleteResults).ToArray();
    }
}