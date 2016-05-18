using System.Linq;
using Catapult.Core.Indexes;
using NUnit.Framework;
using Should;

namespace Catapult.Tests
{
    [TestFixture]
    public class ControlPanelIndexerTests
    {
        [Test]
        public void GetControlPanelItems()
        {
            var indexer = new ControlPanelIndexer();

            var items = indexer.GetControlPanelItems();

            items.Length.ShouldBeGreaterThan(0);
        }
    }
}