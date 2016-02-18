namespace AlphaLaunch.Core.Selecta
{
    public class SelectaSearcher
    {
        public MatchScore Score(string searchString, string targetString)
        {
            var firstSearchChar = searchString[0];
            var restSearchChars = searchString.Substring(1);

            targetString = targetString.ToLowerInvariant();

            var bestScore = int.MaxValue;
            Range bestRange = null;

            for (var i = 0; i < targetString.Length; i++)
            {
                if (targetString[i] != firstSearchChar)
                {
                    continue;
                }

                var score = 1;
                var match = FindEndOfMatch(targetString, restSearchChars, score, i);

                if (match == null)
                {
                    continue;
                }

                if (match.Score < bestScore)
                {
                    bestScore = match.Score;
                    bestRange = match.Range;
                }
            }

            return MatchScore.Create(bestScore, bestRange);
        }

        public class MatchScore
        {
            public Range Range { get; }
            public int Score { get; }

            private MatchScore(int score, Range range)
            {
                Range = range;
                Score = score;
            }

            public static MatchScore Create(int score, Range range)
            {
                return new MatchScore(score, range);
            }
        }

        enum MatchType
        {
            Undefined,
            Normal,
            Sequential,
            Boundary
        }

        private MatchScore FindEndOfMatch(string targetString, string chars, int score, int firstIndex)
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
            }

            return MatchScore.Create(score, new Range(firstIndex, lastIndex));
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