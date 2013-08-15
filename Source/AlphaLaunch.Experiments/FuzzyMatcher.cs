using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Experiments
{
    public class FuzzyMatcher
    {
        private readonly SearchIndex _index;

        public FuzzyMatcher(SearchIndex index)
        {
            _index = index;
        }

        public ImmutableList<Result> Find(string searchString)
        {
            var results = ImmutableList.CreateBuilder<Result>();

            searchString = searchString.ToLowerInvariant();

            foreach (var entry in _index.Entries)
            {
                double boost = 0;
                int lastIndex = 0;
                bool noMatch = false;

                var skips = new Dictionary<char, int>();
                var matchedIndexes = new HashSet<int>();

                foreach (var searchChar in searchString)
                {
                    int skipCount;
                    skips.TryGetValue(searchChar, out skipCount);
                    
                    ImmutableList<int> charSequence;

                    if (!entry.CharLookup.TryGetValue(searchChar, out charSequence))
                    {
                        noMatch = true;
                        break;
                    }

                    if (skipCount + 1 > charSequence.Count)
                    {
                        noMatch = true;
                        break;
                    }

                    var charIndex = charSequence.Skip(skipCount).First();

                    if (charIndex < lastIndex)
                    {
                        noMatch = true;
                        break;
                    }

                    skips[searchChar] = skipCount + 1;

                    if (entry.Boundaries.Contains(charIndex - 1) 
                        || entry.CapitalLetters.Contains(charIndex))
                    {
                        boost += 10;
                    }

                    boost += (11 - (charIndex - lastIndex));

                    matchedIndexes.Add(charIndex);
                    lastIndex = charIndex;
                }

                if (noMatch)
                {
                    continue;
                }

                results.Add(new Result(entry.InputString, (1000 * (100 + boost)) / 100, matchedIndexes));
            }

            return results
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.MatchedString.Length)
                .ToImmutableList();
        }
    }
}