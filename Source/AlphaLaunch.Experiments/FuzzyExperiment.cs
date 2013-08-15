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
            AssertMatches("as", "abc");
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
                .ToArray();
        }
    }
}