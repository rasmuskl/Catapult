using System;
using System.Linq;
using System.Collections.Generic;
using AlphaLaunch.Core.Indexes;
using Should;
using Xunit;

namespace AlphaLaunch.Experiments
{
    public class FuzzyMatcherTests
    {
        [Fact]
        public void Matches_Consecutive()
        {
            AssertMatches("abc", "abc");
            AssertMatches("a", "abc");
            AssertMatches("b", "abc");
            AssertMatches("c", "abc");
        }        
        
        [Fact]
        public void Matches_Casing()
        {
            AssertMatches("ABC", "abc");
            AssertMatches("ABC", "aBc");
            AssertMatches("abc", "ABC");
        }

        [Fact]
        public void Matches_NonConsecutive()
        {
            AssertMatches("ac", "abc");
        }

        [Fact]
        public void NoMatch_NotContained()
        {
            AssertNoMatches("d", "abc");
        }

        [Fact]
        public void NoMatch_PartiallyNotContained()
        {
            AssertNoMatches("ad", "abc");
        }

        [Fact]
        public void NoMatch_WrongOrder()
        {
            AssertNoMatches("ba", "abc");
        }

        [Fact]
        public void Match_SameLetters()
        {
            AssertMatches("aa", "aa");
        }

        [Fact]
        public void NoMatch_TooManySameLetters()
        {
            AssertNoMatches("aaa", "aa");
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

        [Fact]
        public void Rank_SymbolBoundary()
        {
            AssertRankOrder("ab", "axx bxx", "acb");
            AssertRankOrder("ab", "axx/bxx", "acb");
            AssertRankOrder("ab", "axx\\bxx", "acb");
            AssertRankOrder("ab", "axx.bxx", "acb");
            AssertRankOrder("ab", "axx-bxx", "acb");
            AssertRankOrder("ab", "axx_bxx", "acb");
        }

        [Fact]
        public void Rank_BestMatch()
        {
            AssertRankOrder("tsw", "This Secret World", "The Sewer");
        }

        [Fact]
        public void Regressions()
        {
            AssertMatches("clie", "OpenVPN Client.lnk");

            AssertRankOrder("word", "Microsoft Office Word 2007", "The Secret World");
        }

        [Fact]
        public void Rank_CasingBoundary()
        {
            AssertRankOrder("ab", "AxxBxx", "acb");
        }

        private void AssertRankOrder(string searchString, string firstLong, string secondLong)
        {
            var strings = new[] { firstLong, secondLong };
            var matcher = new FuzzyMatcher(new SearchIndex(strings.ToStringIndexables()));
            var reverseMatcher = new FuzzyMatcher(new SearchIndex(strings.Reverse().ToStringIndexables()));

            var results = matcher.Find(searchString);
            var reversedResults = reverseMatcher.Find(searchString);

            PrintHeader(searchString);
            PrintResults(results);

            PrintHeader(searchString + " (reversed)");
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
            var matcher = new FuzzyMatcher(new SearchIndex(new[] { longString }.ToStringIndexables()));

            var results = matcher.Find(searchString);

            PrintHeader(searchString);
            PrintResults(results);

            results.ShouldNotBeNull();
            results.Count().ShouldEqual(0);
        }

        private static void PrintResults(IEnumerable<Result> results)
        {
            foreach (var result in results)
            {
                Console.WriteLine("{0} (score: {1})", result.MatchedString, result.Score);

                var highlight = new string(Enumerable.Range(0, result.MatchedString.Length)
                    .Select(x => result.MatchedIndexes.ContainsKey(x) ? '^' : ' ')
                    .ToArray());

                var charBoostScores = result.MatchedIndexes
                    .OrderBy(x => x.Key)
                    .Select(x => x.Value.ToString("0"));

                highlight += string.Format(" [ {0} ]", string.Join(", ", charBoostScores));

                Console.WriteLine(highlight);
            }
        }

        private void AssertMatches(string searchString, string longString)
        {
            var matcher = new FuzzyMatcher(new SearchIndex(new[] { longString }.ToStringIndexables()));

            var results = matcher.Find(searchString);

            PrintHeader(searchString);
            PrintResults(results);

            results.ShouldNotBeNull();
            results.Count().ShouldBeGreaterThan(0);
            results.First().MatchedString.ShouldEqual(longString);
        }

        private bool _isFirst = true;

        private void PrintHeader(string searchString)
        {
            if (!_isFirst)
            {
                Console.WriteLine();
            }

            Console.WriteLine("Search for: " + searchString);
            _isFirst = false;
        }
    }
}