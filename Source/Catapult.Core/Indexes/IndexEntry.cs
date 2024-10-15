using System.Collections.Immutable;

namespace Catapult.Core.Indexes;

public class IndexEntry
{
    private readonly ImmutableDictionary<char, ImmutableList<int>> _charLookup;
    private readonly ImmutableList<int> _boundaries;
    private readonly ImmutableList<int> _capitalLetters;

    public string InputString { get; private set; }
    public IIndexable Target { get; private set; }

    public ImmutableDictionary<char, ImmutableList<int>> CharLookup
    {
        get { return _charLookup; }
    }

    public ImmutableList<int> Boundaries
    {
        get { return _boundaries; }
    }

    public ImmutableList<int> CapitalLetters
    {
        get { return _capitalLetters; }
    }

    public IndexEntry(string inputString, IIndexable target)
    {
        InputString = inputString;
        Target = target;

        _charLookup = inputString
            .Select((x, i) => Tuple.Create(x, i))
            .ToLookup(x => char.ToLowerInvariant(x.Item1), x => x.Item2)
            .ToImmutableDictionary(x => x.Key, x => x.ToImmutableList());

        _boundaries = ImmutableList.CreateRange(inputString
            .Select((x, i) => new { Char = x, Index = i })
            .Where(x => @" -_\/.".Contains(x.Char))
            .Select(x => x.Index)
            .Concat(new [] { -1 }));

        _capitalLetters = ImmutableList.CreateRange(inputString
            .Select((x, i) => new { Char = x, Index = i })
            .Where(x => char.IsUpper(x.Char))
            .Select(x => x.Index));
    }
}