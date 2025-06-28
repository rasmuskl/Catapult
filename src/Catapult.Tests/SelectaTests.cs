using Catapult.Core.Selecta;

namespace Catapult.Tests;

[TestFixture]
public class SelectaTests
{
    //[Test]
    //public void Test()
    //{
    //    var startNew = Stopwatch.StartNew();
    //    var enumerateFiles = SafeWalk.EnumerateFiles(@"c:\", "*", SearchOption.AllDirectories);
    //    startNew.Stop();

    //    var startNew2 = Stopwatch.StartNew();
    //    var count = enumerateFiles.Count();
    //    startNew2.Stop();

    //    Console.WriteLine("{0}ms - {1} files - {2}ms", startNew.ElapsedMilliseconds, count, startNew2.ElapsedMilliseconds);
    //}

    [TestCase("a", "a", 100)]
    [TestCase("a", "b", int.MaxValue)]
    [TestCase("ln", "length", 300)]
    [TestCase("am", "app/models", 100)]
    [TestCase("am", "appm/models", 100)]
    [TestCase("spec", "spearch_spec.rb", 100)]
    [TestCase("amu", "app/models/user.rb", 100)]
    [TestCase("a.b", "app.bok", 100)]
    [TestCase("word", "Microsoft Word", 100)]
    [TestCase("lice", "LICE.rb", 100)]
    [TestCase("vstud", "SQL Server Management Studio", 1600)]
    public void ScoreFunction(string searchString, string targetString, int expectedScore)
    {
        var score = new SelectaSearcher().Score2(searchString, targetString);
        Assert.AreEqual(expectedScore, score.Score);
    }
}