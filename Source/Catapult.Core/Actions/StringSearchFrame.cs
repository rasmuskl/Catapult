using System.Collections.Immutable;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class StringSearchFrame : ISearchFrame
    {
        public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
        {
            return new[] { new SearchResult(search, 0, new StringIndexable(search), ImmutableHashSet.Create<int>()),  };
        }
    }
}