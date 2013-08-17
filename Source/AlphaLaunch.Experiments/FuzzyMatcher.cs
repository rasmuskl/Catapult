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
                int lastIndex = -1;
                bool noMatch = false;

                var matchedIndexes = ImmutableDictionary.Create<int, double>();

                foreach (var searchChar in searchString)
                {
                    ImmutableList<int> charIndexes;

                    if (!entry.CharLookup.TryGetValue(searchChar, out charIndexes))
                    {
                        noMatch = true;
                        break;
                    }

                    charIndexes = charIndexes.RemoveAll(x => x <= lastIndex);

                    if (!charIndexes.Any())
                    {
                        noMatch = true;
                        break;
                    }

                    var charIndex = charIndexes.First();

                    if (entry.Boundaries.Contains(charIndex - 1) 
                        || entry.CapitalLetters.Contains(charIndex))
                    {
                        boost += 10;
                    }

                    if (lastIndex != -1)
                    {
                        boost += (11 - (charIndex - lastIndex));
                    }

                    matchedIndexes = matchedIndexes.Add(charIndex, boost);
                    lastIndex = charIndex;
                }

                if (noMatch)
                {
                    continue;
                }

                results.Add(new Result(entry, (1000 * (100 + boost)) / 100, matchedIndexes));
            }

            return results
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.MatchedString.Length)
                .ToImmutableList();
        }
    }
}