using System;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using AlphaLaunch.Core.Indexes;
using NUnit.Framework;
using Should;

namespace AlphaLaunch.Experiments
{
    [TestFixture]
    public class FuzzyMatcherTests
    {
        [Test]
        public void Matches_Consecutive()
        {
            AssertMatches("abc", "abc");
            AssertMatches("a", "abc");
            AssertMatches("b", "abc");
            AssertMatches("c", "abc");
        }

        [Test]
        public void Matches_Casing()
        {
            AssertMatches("ABC", "abc");
            AssertMatches("ABC", "aBc");
            AssertMatches("abc", "ABC");
        }

        [Test]
        public void Matches_NonConsecutive()
        {
            AssertMatches("ac", "abc");
        }

        [Test]
        public void NoMatch_NotContained()
        {
            AssertNoMatches("d", "abc");
        }

        [Test]
        public void NoMatch_PartiallyNotContained()
        {
            AssertNoMatches("ad", "abc");
        }

        [Test]
        public void NoMatch_WrongOrder()
        {
            AssertNoMatches("ba", "abc");
        }

        [Test]
        public void Match_SameLetters()
        {
            AssertMatches("aa", "aa");
        }

        [Test]
        public void NoMatch_TooManySameLetters()
        {
            AssertNoMatches("aaa", "aa");
        }

        [Test]
        public void Rank_Length()
        {
            AssertRankOrder("abc", "abc", "abcd");
        }

        [Test]
        public void Rank_Consecutive()
        {
            AssertRankOrder("abc", "abcd", "abxc");
            AssertRankOrder("abc", "abcdefgh", "abxc");
            AssertRankOrder("aaa", "aaaaa", "aaba");
        }

        [Test]
        public void Rank_SymbolBoundary()
        {
            AssertRankOrder("ab", "axx bxx", "acb");
            AssertRankOrder("ab", "axx/bxx", "acb");
            AssertRankOrder("ab", "axx\\bxx", "acb");
            AssertRankOrder("ab", "axx.bxx", "acb");
            AssertRankOrder("ab", "axx-bxx", "acb");
            AssertRankOrder("ab", "axx_bxx", "acb");
        }

        [Test]
        public void Rank_BestMatch()
        {
            AssertRankOrder("tsw", "This Secret World", "The Sewer");
        }

        [Test]
        public void Rank_Boost()
        {
            var boostDictionary = ImmutableDictionary.Create<string, EntryBoost>()
                .Add("tsw", new EntryBoost("The Sewer"));

            AssertRankOrder("tsw", "The Sewer", "This Secret World", boostDictionary);
        }

        [Test]
        public void Regressions()
        {
            AssertMatches("clie", "OpenVPN Client.lnk");

            AssertRankOrder("word", "Microsoft Office Word 2007", "The Secret World");
        }

        [Test]
        public void Rank_CasingBoundary()
        {
            AssertRankOrder("ab", "AxxBxx", "acb");
        }

        [Test]
        public void Rank_ConsecutiveOverBoundaries()
        {
            AssertRankOrder("ste", "Steam", "Sublime Text 3");
        }

        private void AssertRankOrder(string searchString, string firstLong, string secondLong, ImmutableDictionary<string, EntryBoost> boostDictionary = null)
        {
            var strings = new[] { firstLong, secondLong };
            var matcher = new FuzzyMatcher(new SearchIndex(strings.ToStringIndexables()));
            var reverseMatcher = new FuzzyMatcher(new SearchIndex(strings.Reverse().ToStringIndexables()));

            var results = matcher.Find(searchString, boostDictionary);
            var reversedResults = reverseMatcher.Find(searchString, boostDictionary);

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