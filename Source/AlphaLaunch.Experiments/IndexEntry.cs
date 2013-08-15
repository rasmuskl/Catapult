using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaLaunch.Experiments
{
    public class IndexEntry
    {
        private readonly ILookup<char, int> _charLookup;
        private readonly int[] _boundaries;
        private readonly int[] _capitalLetters;
        
        public string InputString { get; private set; }

        public ILookup<char, int> CharLookup
        {
            get { return _charLookup; }
        }

        public int[] Boundaries
        {
            get { return _boundaries; }
        }

        public int[] CapitalLetters
        {
            get { return _capitalLetters; }
        }

        public IndexEntry(string inputString)
        {
            InputString = inputString;

            _charLookup = inputString
                .Select((x, i) => Tuple.Create(x, i))
                .ToLookup(x => char.ToLowerInvariant(x.Item1), x => x.Item2);

            _boundaries = inputString
                .Select((x, i) => new { Char = x, Index = i })
                .Where(x => @" -_\/.".Contains(x.Char))
                .Select(x => x.Index)
                .ToArray();

            _capitalLetters = inputString
                .Select((x, i) => new { Char = x, Index = i })
                .Where(x => char.IsUpper(x.Char))
                .Select(x => x.Index)
                .ToArray();
        }
    }
}