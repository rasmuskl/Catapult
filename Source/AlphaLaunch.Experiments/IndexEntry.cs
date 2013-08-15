using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Experiments
{
    public class IndexEntry
    {
        private readonly ImmutableDictionary<char, ImmutableList<int>> _charLookup;
        private readonly ImmutableList<int> _boundaries;
        private readonly ImmutableList<int> _capitalLetters;

        public string InputString { get; private set; }

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

        public IndexEntry(string inputString)
        {
            InputString = inputString;

            _charLookup = inputString
                .Select((x, i) => Tuple.Create(x, i))
                .ToLookup(x => char.ToLowerInvariant(x.Item1), x => x.Item2)
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableList());

            _boundaries = ImmutableList.From(inputString
                .Select((x, i) => new { Char = x, Index = i })
                .Where(x => @" -_\/.".Contains(x.Char))
                .Select(x => x.Index));

            _capitalLetters = ImmutableList.From(inputString
                .Select((x, i) => new { Char = x, Index = i })
                .Where(x => char.IsUpper(x.Char))
                .Select(x => x.Index));
        }
    }
}