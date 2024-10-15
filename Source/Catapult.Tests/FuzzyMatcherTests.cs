using Catapult.Core.Selecta;
using Should;

namespace Catapult.Tests;

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
        AssertRankOrder("tsw", "The Secret World", "The Sewer");
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

    private void AssertRankOrder(string searchString, string firstLong, string secondLong)
    {
        var strings = new[] { firstLong, secondLong };
        var matcher = Searcher.Create(strings.ToStringIndexables());
        var reverseMatcher = Searcher.Create(strings.Reverse().ToStringIndexables());

        var results = matcher.Search(searchString);
        var reversedResults = reverseMatcher.Search(searchString);

        PrintHeader(searchString);
        PrintResults(results);

        PrintHeader(searchString + " (reversed)");
        PrintResults(reversedResults);

        results.ShouldNotBeNull();
        results.SearchResults.Count().ShouldEqual(2);
        results.SearchResults[0].Name.ShouldEqual(firstLong);
        results.SearchResults[1].Name.ShouldEqual(secondLong);

        reversedResults.ShouldNotBeNull();
        reversedResults.SearchResults.Count().ShouldEqual(2);
        reversedResults.SearchResults[0].Name.ShouldEqual(firstLong);
        reversedResults.SearchResults[1].Name.ShouldEqual(secondLong);
    }

    private void AssertNoMatches(string searchString, string longString)
    {
        var matcher = Searcher.Create(new[] { longString }.ToStringIndexables());

        var searcher = matcher.Search(searchString);

        PrintHeader(searchString);
        PrintResults(searcher);

        searcher.SearchResults.Length.ShouldEqual(0);
    }

    private static void PrintResults(Searcher searcher)
    {
        foreach (var result in searcher.SearchResults)
        {
            Console.WriteLine("{0} (score: {1})", result.Name, result.Score);

            var highlight = new string(Enumerable.Range(0, result.Name.Length)
                .Select(x => result.HighlightIndexes.Contains(x) ? '^' : ' ')
                .ToArray());

            Console.WriteLine(highlight);
        }
    }

    private void AssertMatches(string searchString, string longString)
    {
        var matcher = Searcher.Create(new[] { longString }.ToStringIndexables());

        var searcher = matcher.Search(searchString);

        PrintHeader(searchString);
        PrintResults(searcher);

        searcher.ShouldNotBeNull();
        searcher.SearchResults.Count().ShouldBeGreaterThan(0);
        searcher.SearchResults.First().Name.ShouldEqual(longString);
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