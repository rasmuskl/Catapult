using System;
using System.Linq;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;
using Serilog;

namespace Catapult.Core.Actions
{
    public class UpdateableIndexableSearchFrame : ISearchFrame
    {
        private IndexableUpdateState _indexableUpdateState;
        private Searcher _selectaSeacher;

        public UpdateableIndexableSearchFrame(IndexableUpdateState indexableUpdateState)
        {
            _indexableUpdateState = indexableUpdateState;
            _selectaSeacher = Searcher.Create(_indexableUpdateState.Indexables);
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

            return _selectaSeacher.SearchResults.Take(100).ToArray();
        }
    }

    public class IndexableUpdateState
    {
        private readonly Func<IndexableResult> _fetchIndexables;
        private readonly Func<string> _getUpdatedState;
        private readonly IndexableResult _indexableResult;

        public IIndexable[] Indexables => _indexableResult?.Indexables;

        public IndexableUpdateState(Func<IndexableResult> fetchIndexables, Func<string> getUpdatedState)
        {
            _fetchIndexables = fetchIndexables;
            _getUpdatedState = getUpdatedState;
            _indexableResult = _fetchIndexables();
        }

        public IndexableUpdateState CheckUpdate(out bool updated)
        {
            if (_getUpdatedState() == _indexableResult.State)
            {
                updated = false;
                return this;
            }

            updated = true;
            return new IndexableUpdateState(_fetchIndexables, _getUpdatedState);
        }
    }

    public class IndexableResult
    {
        public IIndexable[] Indexables { get; set; }
        public string State { get; set; }

        public IndexableResult(IIndexable[] indexables, string state)
        {
            Indexables = indexables;
            State = state;
        }
    }
}