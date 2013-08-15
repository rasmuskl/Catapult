using System;
using System.Linq;
using System.Collections.Generic;
using Should;
using Xunit;

namespace AlphaLaunch.Experiments
{
    public class FuzzyExperiment
    {
        [Fact]
        public void SimpleMatches()
        {
            AssertMatches("abc", "abc");
            AssertMatches("a", "abc");
            AssertMatches("b", "abc");
            AssertMatches("c", "abc");
        }

        [Fact]
        public void SimpleNoMatches()
        {
            AssertNoMatches("d", "abc");
        }

        [Fact]
        public void Matches()
        {
            AssertMatches("ac", "abc");
        }

        [Fact]
        public void NoMatches()
        {
            AssertNoMatches("ad", "abc");
            AssertNoMatches("ba", "abc");
        }

        [Fact]
        public void Rank_Length()
        {
            AssertRankOrder("abc", "abc", "abcd");
        }

        [Fact]
        public void Rank_Consecutive()
        {
            AssertRankOrder("abc", "abcd", "abxc");
            AssertRankOrder("abc", "abcdefgh", "abxc");
        }

        public class FuzzyMatcher
        {
            public Result[] Find(string searchString, string[] strings)
            {
                var result = new Dictionary<string, double>();

                foreach (var str in strings)
                {
                    var charIndexes = searchString
                        .Select(c => str.IndexOf(c))
                        .ToArray();

                    if (charIndexes.Contains(-1))
                    {
                        continue;
                    }

                    double boost = 0;

                    int lastIndex = 0;
                    bool noMatch = false;

                    foreach (var charIndex in charIndexes)
                    {
                        if (charIndex < lastIndex)
                        {
                            noMatch = true;
                            break;
                        }

                        boost += (11 - (charIndex - lastIndex));

                        lastIndex = charIndex;
                    }

                    if (noMatch)
                    {
                        continue;
                    }

                    result[str] = (1000 * (100 + boost)) / 100;
                }

                return result
                    .Select(x => new Result(x.Key, x.Value))
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.MatchedString.Length)
                    .ToArray();
            }
        }

        private void AssertRankOrder(string searchString, string firstLong, string secondLong)
        {
            var matcher = new FuzzyMatcher();

            var strings = new[] { firstLong, secondLong };

            var results = matcher.Find(searchString, strings);
            var reversedResults = matcher.Find(searchString, strings.Reverse().ToArray());
            
            PrintResults(results);
            PrintResults(reversedResults);

            results.ShouldNotBeNull();
            results.Count().ShouldEqual(2);
            results[0].MatchedString.ShouldEqual(firstLong);
            results[1].MatchedString.ShouldEqual(secondLong);

            reversedResults.ShouldNotBeNull();
            reversedResults.Count().ShouldEqual(2);
            reversedResults[0].MatchedString.ShouldEqual(firstLong);
            reversedResults[1].MatchedString.ShouldEqual(secondLong);
        }

        private void AssertNoMatches(string searchString, string longString)
        {
            var matcher = new FuzzyMatcher();

            var results = matcher.Find(searchString, new[] { longString });

            PrintResults(results);

            results.ShouldNotBeNull();
            results.Count().ShouldEqual(0);
        }

        private static void PrintResults(IEnumerable<Result> results)
        {
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
        }

        private void AssertMatches(string searchString, string longString)
        {
            var matcher = new FuzzyMatcher();

            var results = matcher.Find(searchString, new[] { longString });

            PrintResults(results);

            results.ShouldNotBeNull();
            results.Count().ShouldBeGreaterThan(0);
            results.First().MatchedString.ShouldEqual(longString);
        }
    }

    public class Result
    {
        public string MatchedString { get; private set; }
        public double Score { get; private set; }

        public Result(string matchedString, double score)
        {
            MatchedString = matchedString;
            Score = score;
        }

        public override string ToString()
        {
            return string.Format("MatchedString: {0}, Score: {1}", MatchedString, Score);
        }
    }
}