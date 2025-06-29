using System.Collections.Immutable;

namespace Catapult.Core.Indexes;

public class SearchResult(string name, double score, IIndexable targetItem, ImmutableHashSet<int> highlightIndexes)
{
    public string Name { get; private set; } = name;
    public double Score { get; private set; } = score;
    public IIndexable TargetItem { get; private set; } = targetItem;
    public ImmutableHashSet<int> HighlightIndexes { get; private set; } = highlightIndexes;
}