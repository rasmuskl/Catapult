using System.Collections.Immutable;

namespace Catapult.Core.Indexes;

public class Result
{
    private readonly IndexEntry _indexEntry;

    public double Score { get; private set; }
    public ImmutableDictionary<int, double> MatchedIndexes { get; private set; }

    public string MatchedString
    {
        get { return _indexEntry.InputString; }
    }

    public IIndexable TargetItem
    {
        get { return _indexEntry.Target; }
    }

    public Result(IndexEntry indexEntry, double score, ImmutableDictionary<int, double> matchedIndexes)
    {
        _indexEntry = indexEntry;
        Score = score;
        MatchedIndexes = matchedIndexes;
    }

    public Result(Result result, double score) : this(result._indexEntry, score, result.MatchedIndexes)
    {
    }

    public override string ToString()
    {
        return string.Format("MatchedString: {0}, Score: {1}", MatchedString, Score);
    }
}