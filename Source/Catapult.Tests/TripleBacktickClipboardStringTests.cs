using Catapult.Core.Actions;
using NUnit.Framework;
using Should;

namespace Catapult.Tests
{
    [TestFixture]
    public class TripleBacktickClipboardStringTests
    {
        [Test]
        public void TrimSharedWhitespacePrefix_NoWhitespaceNoChange()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix("test");
            test.ShouldEqual("test");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_SimpleWhitespaceRemoved()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix("   test");
            test.ShouldEqual("test");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_TabWhitespaceRemoved()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix("   test");
            test.ShouldEqual("test");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_MultilineWhitespaceRemoved()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix(@"   test
   test");
            test.ShouldEqual(@"test
test");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_MultilineWhitespaceRemovesOnlyShared()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix(@" test
  test");
            test.ShouldEqual(@"test
 test");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_MultilineWhitespaceIgnoresEmptyLines()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix(@"  test

  test");
            test.ShouldEqual(@"test

test");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_EmptyStringWorks()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix(@"");
            test.ShouldEqual(@"");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_MultilineWhitespaceStringWorks()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix(@"


");
            test.ShouldEqual(@"


");
        }

        [Test]
        public void TrimSharedWhitespacePrefix_NullWorks()
        {
            var test = TripleBacktickClipboardString.TrimSharedWhitespacePrefix(null);
            test.ShouldEqual(null);
        }
    }
}