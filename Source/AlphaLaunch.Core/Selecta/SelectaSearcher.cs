using System.Collections.Immutable;

namespace AlphaLaunch.Core.Selecta
{
    public class SelectaSearcher
    {
        public MatchScore Score(string searchString, string targetString)
        {
            var firstSearchChar = searchString[0];
            var restSearchChars = searchString.Substring(1);

            targetString = targetString.ToLowerInvariant();

            MatchScore bestMatch = null;

            for (var i = 0; i < targetString.Length; i++)
            {
                if (targetString[i] != firstSearchChar)
                {
                    continue;
                }

                var score = 1;
                var match = FindEndOfMatch(targetString, restSearchChars, score, i, ImmutableHashSet.Create(i));

                if (match == null)
                {
                    continue;
                }

                if (match.Score < (bestMatch?.Score ?? int.MaxValue))
                {
                    bestMatch = match;
                }
            }

            return bestMatch;
        }

        public class MatchScore
        {
            public Range Range { get; }
            public ImmutableHashSet<int> MatchSet { get; }
            public int Score { get; }

            private MatchScore(int score, Range range, ImmutableHashSet<int> matchSet)
            {
                Range = range;
                MatchSet = matchSet;
                Score = score;
            }

            public static MatchScore Create(int score, Range range, ImmutableHashSet<int> matchSet)
            {
                return new MatchScore(score, range, matchSet);
            }
        }

        enum MatchType
        {
            Undefined,
            Normal,
            Sequential,
            Boundary
        }

        private MatchScore FindEndOfMatch(string targetString, string chars, int score, int firstIndex, ImmutableHashSet<int> matchSet)
        {
            var lastIndex = firstIndex;
            var lastMatch = MatchType.Undefined;

            foreach (var c in chars)
            {
                var index = targetString.IndexOf(c, lastIndex + 1);

                if (index == -1)
                {
                    return null;
                }

                if (index == lastIndex + 1)
                {
                    if (lastMatch != MatchType.Sequential)
                    {
                        score += 1;
                        lastMatch = MatchType.Sequential;
                    }
                }
                else if (!char.IsLetterOrDigit(targetString[index - 1]))
                {
                    if (lastMatch != MatchType.Boundary)
                    {
                        score += 1;
                        lastMatch = MatchType.Boundary;
                    }
                }
                else
                {
                    score += index - lastIndex;
                    lastMatch = MatchType.Normal;
                }

                lastIndex = index;
                matchSet = matchSet.Add(index);
            }

            return MatchScore.Create(score, new Range(firstIndex, lastIndex), matchSet);
        }

        public class Range
        {
            public int StartIndex { get; }
            public int EndIndex { get; }

            public Range(int startIndex, int endIndex)
            {
                StartIndex = startIndex;
                EndIndex = endIndex;
            }
        }
    }
}