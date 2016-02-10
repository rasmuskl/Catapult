using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AlphaLaunch.Core.Selecta;
using NUnit.Framework;

namespace AlphaLaunch.Experiments
{
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

        [TestCase("a", "a", 1)]
        [TestCase("a", "b", int.MaxValue)]
        [TestCase("ln", "length", 3)]
        [TestCase("am", "app/models", 2)]
        //[TestCase("am", "appm/models", 2)]
        [TestCase("spec", "spearch_spec.rb", 2)]
        [TestCase("amu", "app/models/user.rb", 2)]
        [TestCase("lice", "LICE.rb", 2)]
        public void ScoreFunction(string searchString, string targetString, int expectedScore)
        {
            var score = new SelectaSearcher().Score(searchString, targetString);
            Assert.AreEqual(expectedScore, score.Score);
        }

    }
}