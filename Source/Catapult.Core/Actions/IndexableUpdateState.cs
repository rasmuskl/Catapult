using System;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
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
}