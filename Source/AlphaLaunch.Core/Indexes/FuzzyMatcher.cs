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

        public ImmutableList<Result> Find(string searchString, ImmutableDictionary<string, EntryBoost> boostDictionary = null)
        {
            var results = ImmutableList.CreateBuilder<Result>();

            searchString = searchString.ToLowerInvariant();

            foreach (var entry in _index.Entries)
            {
                var matchedIndexes = ImmutableHashSet.Create<int>();

                var result = GetBestMatch(searchString, entry, int.MinValue, 0, matchedIndexes, 0);

                if (result == null)
                {
                    continue;
                }

                results.Add(result);
            }

            if (boostDictionary != null && boostDictionary.ContainsKey(searchString))
            {
                var entryBoost = boostDictionary[searchString];
                var boostedResult = results.FirstOrDefault(x => x.TargetItem.BoostIdentifier == entryBoost.BoostIdentifier);

                if (boostedResult != null)
                {
                    var maxScore = results.Except(new [] { boostedResult }).Max(x => x.Score);

                    var index = results.IndexOf(boostedResult);
                    results[index] = new Result(boostedResult, maxScore + 100);
                }
            }

            return results
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.MatchedString.Length)
                .ToImmutableList();
        }

        private static Result GetBestMatch(string searchString, IndexEntry entry, int lastIndex, int consecutiveChars, ImmutableHashSet<int> matchedIndexes, double boost)
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

            return MatchNextChar(searchString, entry, lastIndex, consecutiveChars, matchedIndexes, boost, charIndexes);
        }

        private static Result MatchNextChar(string searchString, IndexEntry entry, int lastIndex, int consecutiveChars, ImmutableHashSet<int> matchedIndexes, double boost, ImmutableList<int> charIndexes)
        {
            double maxScore = 0;
            Result best = null;

            foreach (var charIndex in charIndexes)
            {
                var charBoost = 0;

                if (entry.Boundaries.Contains(charIndex - 1) || entry.CapitalLetters.Contains(charIndex))
                {
                    charBoost += 10;
                }

                if (lastIndex == charIndex - 1)
                {
                    consecutiveChars += 1;
                    charBoost += 10 * consecutiveChars;
                }
                else
                {
                    consecutiveChars = 0;
                }

                var charMatchedIndexes = matchedIndexes.Add(charIndex);

                var result = GetBestMatch(searchString.Substring(1), entry, charIndex, consecutiveChars, charMatchedIndexes, boost + charBoost);

                if (result != null && result.Score > maxScore)
                {
                    best = result;
                    maxScore = best.Score;
                }
            }

            return best;
        }
    }
}