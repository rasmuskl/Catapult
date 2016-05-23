using System.Collections.Immutable;
using System.Linq;

namespace Catapult.Core.Selecta
{
    public class SelectaSearcher
    {
        private static readonly SelectaMode Mode = SelectaMode.Strict;
        private static readonly MatchScore NoMatch = MatchScore.Create(int.MaxValue, new Range(0, 0), ImmutableHashSet.Create<int>());

        public MatchScore Score(string searchString, string targetString)
        {
            var firstSearchChar = searchString[0];
            var restSearchChars = searchString.Substring(1);

            targetString = targetString.ToLowerInvariant();

            MatchScore bestMatch = NoMatch;

            for (var i = 0; i < targetString.Length; i++)
            {
                if (targetString[i] != firstSearchChar)
                {
                    continue;
                }

                var score = 1;
                var match = FindEndOfMatch(targetString, restSearchChars, score, i, ImmutableHashSet.Create(i));

                if (match?.Score < bestMatch.Score)
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

                if (score < int.MaxValue)
                {
                    Score = score * 100;
                }
                else
                {
                    Score = score;
                }
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

            if (firstIndex == 0 || !char.IsLetterOrDigit(targetString[firstIndex - 1]))
            {
                lastMatch = MatchType.Boundary;
            }

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

        public MatchScore Score2(string searchString, string targetString)
        {
            return GetBestMatch(searchString, targetString, null, 1, ImmutableHashSet.Create<int>(), MatchType.Normal);
        }

        private static MatchScore GetBestMatch(string searchString, string targetString, int? lastIndex, int score, ImmutableHashSet<int> matchedIndexes, MatchType lastMatch)
        {
            if (searchString.Length == 0)
            {
                return MatchScore.Create(score, new Range(0, 0), matchedIndexes);
            }

            var searchChar = char.ToLowerInvariant(searchString.First());

            var bestMatch = NoMatch;

            for (var i = lastIndex + 1 ?? 0; i < targetString.Length; i++)
            {
                if (char.ToLowerInvariant(targetString[i]) == searchChar)
                {
                    MatchType matchType;
                    var nextScore = score;

                    if (i == lastIndex + 1)
                    {
                        matchType = MatchType.Sequential;
                    }
                    else if (i == 0 || !char.IsLetterOrDigit(targetString[i - 1]) || !char.IsLetterOrDigit(targetString[i]))
                    {
                        matchType = MatchType.Boundary;
                    }
                    else if (char.IsLower(targetString[i - 1]) && char.IsUpper(targetString[i]))
                    {
                        matchType = MatchType.Boundary;
                    }
                    else
                    {
                        matchType = MatchType.Normal;
                    }

                    if (lastMatch == MatchType.Normal)
                    {
                        if (lastIndex.HasValue)
                        {
                            nextScore += i - lastIndex.Value;
                        }
                    }
                    else if (lastMatch == MatchType.Boundary || lastMatch == MatchType.Sequential)
                    {
                        if (matchType == MatchType.Normal)
                        {
                            if (lastIndex.HasValue)
                            {
                                nextScore += i - lastIndex.Value;
                            }
                        }
                    }

                    if (Mode == SelectaMode.Strict && matchType == MatchType.Normal)
                    {
                        continue;
                    }

                    var match = GetBestMatch(searchString.Substring(1), targetString, i, nextScore, matchedIndexes.Add(i), matchType);

                    if (match.Score < bestMatch.Score)
                    {
                        bestMatch = match;
                    }
                }
            }

            return bestMatch;
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

    public enum SelectaMode
    {
        Normal,
        Strict
    }
}