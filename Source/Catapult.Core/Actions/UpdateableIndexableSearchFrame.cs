using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;
using Jace;
using Serilog;

namespace Catapult.Core.Actions
{
    public class UpdateableIndexableSearchFrame : ISearchFrame
    {
        private IndexableUpdateState _indexableUpdateState;
        private Searcher _selectaSeacher;
        private readonly CalculationEngine _calculationEngine;

        public UpdateableIndexableSearchFrame(IndexableUpdateState indexableUpdateState)
        {
            _indexableUpdateState = indexableUpdateState;
            _selectaSeacher = Searcher.Create(_indexableUpdateState.Indexables);
            _calculationEngine = new CalculationEngine();
        }

        public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
        {
            bool updated;
            _indexableUpdateState = _indexableUpdateState.CheckUpdate(out updated);

            if (updated)
            {
                Log.Information("Updated searcher with new indexables.");
                _selectaSeacher = Searcher.Create(_indexableUpdateState.Indexables);
            }

            var frecencyData = frecencyStorage.GetFrecencyData();
            Func<IIndexable, int> boosterFunc = x => frecencyData.ContainsKey(x.BoostIdentifier) ? frecencyData[x.BoostIdentifier] : 0;
            _selectaSeacher = _selectaSeacher.Search(search, boosterFunc);

            SearchResult[] searchResults = _selectaSeacher.SearchResults.Take(100).ToArray();

            try
            {
                double result = _calculationEngine.Calculate(search);
                var name = result.ToString(CultureInfo.InvariantCulture);

                if (!string.Equals(name, search?.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    searchResults = searchResults.Concat(new[] {new SearchResult(name, 0, new StringIndexable(name, "Result of formula"), ImmutableHashSet.Create<int>())}).ToArray();
                }
            }
            catch
            {
                // Ignore calculation errors.
            }

            return searchResults;
        }
    }
}