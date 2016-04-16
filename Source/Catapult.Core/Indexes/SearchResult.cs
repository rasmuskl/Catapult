using System.Collections.Immutable;

namespace Catapult.Core.Indexes
{
    public class SearchResult
    {
        public SearchResult(string name, double score, IIndexable targetItem, ImmutableHashSet<int> highlightIndexes)
        {
            Name = name;
            Score = score;
            TargetItem = targetItem;
            HighlightIndexes = highlightIndexes;
        }

        public string Name { get; private set; }
        public double Score { get; private set; }
        public IIndexable TargetItem { get; private set; }
        public ImmutableHashSet<int> HighlightIndexes { get; private set; }
    }
}