using System.Collections.Immutable;

namespace Catapult.Core.Indexes;

public class Result
{
    private readonly IndexEntry _indexEntry;

    public double Score { get; private set; }
    public ImmutableDictionary<int, double> MatchedIndexes { get; private set; }

    public string MatchedString => _indexEntry.InputString;

    public IIndexable TargetItem => _indexEntry.Target;

    public Result(Result result, double score) : this(result._indexEntry, score, result.MatchedIndexes)
    {
    }

    private Result(IndexEntry indexEntry, double score, ImmutableDictionary<int, double> matchedIndexes)
    {
        _indexEntry = indexEntry;
        Score = score;
        MatchedIndexes = matchedIndexes;
    }

    public override string ToString()
    {
        return $"MatchedString: {MatchedString}, Score: {Score}";
    }
}