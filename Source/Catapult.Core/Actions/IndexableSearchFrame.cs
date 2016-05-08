using System;
using System.Linq;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions
{
    public class IndexableSearchFrame : ISearchFrame
    {
        private Searcher _selectaSeacher;

        public IndexableSearchFrame(IIndexable[] indexables)
        {
            _selectaSeacher = Searcher.Create(indexables);
        }

        public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
        {
            var frecencyData = frecencyStorage.GetFrecencyData();
            Func<IIndexable, int> boosterFunc = x => frecencyData.ContainsKey(x.BoostIdentifier) ? frecencyData[x.BoostIdentifier] : 0;
            _selectaSeacher = _selectaSeacher.Search(search, boosterFunc);
            var searchResults = _selectaSeacher.SearchResults.Take(10);
            return searchResults.ToArray();
        }
    }
}