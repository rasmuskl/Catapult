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
        }

        [Fact]
        public void Rank()
        {
            AssertRankOrder("abc", "abc", "abcd");
        }

        private void AssertRankOrder(string searchString, string firstLong, string secondLong)
        {
            var matcher = new FuzzyMatcher();

            var strings = new[] { firstLong, secondLong };

            var results = matcher.Find(searchString, strings);
            var reversedResults = matcher.Find(searchString, strings.Reverse().ToArray());

            results.ShouldNotBeNull();
            results.Count().ShouldEqual(2);
            results[0].ShouldEqual(firstLong);
            results[1].ShouldEqual(secondLong);

            reversedResults.ShouldNotBeNull();
            reversedResults.Count().ShouldEqual(2);
            reversedResults[0].ShouldEqual(firstLong);
            reversedResults[1].ShouldEqual(secondLong);
        }

        private void AssertNoMatches(string searchString, string longString)
        {
            var matcher = new FuzzyMatcher();

            var results = matcher.Find(searchString, new[] { longString });

            results.ShouldNotBeNull();
            results.Count().ShouldEqual(0);
        }

        private void AssertMatches(string searchString, string longString)
        {
            var matcher = new FuzzyMatcher();

            var results = matcher.Find(searchString, new[] { longString });

            results.ShouldNotBeNull();
            results.Count().ShouldBeGreaterThan(0);
            results.First().ShouldEqual(longString);
        }
    }

    public class FuzzyMatcher
    {
        public string[] Find(string searchString, string[] strings)
        {
            return strings
                .Where(x => searchString.All(x.Contains))
                .OrderBy(x => x.Length)
                .ToArray();
        }
    }
}