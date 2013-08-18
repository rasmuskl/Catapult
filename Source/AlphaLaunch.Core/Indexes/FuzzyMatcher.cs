using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Core.Indexes
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

                var matchedIndexes = ImmutableDictionary.Create<int, double>();

                var result = GetMatch(searchString, entry, lastIndex, matchedIndexes, boost);

                if (result == null)
                {
                    continue;
                }

                results.Add(result);
            }

            return results
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.MatchedString.Length)
                .ToImmutableList();
        }

        private static Result GetMatch(string searchString, IndexEntry entry, int lastIndex, ImmutableDictionary<int, double> matchedIndexes, double boost)
        {
            if (searchString.Length == 0)
            {
                return new Result(entry, (1000 * (100 + boost)) / 100, matchedIndexes);
            }
            
            var searchChar = searchString.First();

            ImmutableList<int> charIndexes;

            if (!entry.CharLookup.TryGetValue(searchChar, out charIndexes))
            {
                return null;
            }

            charIndexes = charIndexes.RemoveAll(x => x <= lastIndex);

            if (!charIndexes.Any())
            {
                return null;
            }

            var charBoost = 0;
            var charIndex = charIndexes.First();

            if (entry.Boundaries.Contains(charIndex - 1)
                || entry.CapitalLetters.Contains(charIndex))
            {
                charBoost += 10;
            }

            if (lastIndex != -1)
            {
                charBoost += (11 - (charIndex - lastIndex));
            }

            matchedIndexes = matchedIndexes.Add(charIndex, boost);
            lastIndex = charIndex;

            boost += charBoost;

            return GetMatch(searchString.Substring(1), entry, lastIndex, matchedIndexes, boost);
        }
    }
}