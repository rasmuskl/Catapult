using System.Linq;
using Catapult.Core;
using Catapult.Core.Actions;
using NUnit.Framework;
using Should;

namespace Catapult.Tests
{
    [TestFixture]
    public class ActionRegistryTests
    {
        [Test]
        public void GetActionFor_FileItem()
        {
            var actionRegistry = new ActionRegistry();

            actionRegistry.RegisterAction<OpenAction>();
            actionRegistry.RegisterAction<ContainingFolderAction>();
            actionRegistry.RegisterAction<GoogleAction>();

            var actions = actionRegistry.GetActionFor(typeof(FileItem));

            actions.Count.ShouldEqual(2);

            actions.Any(x => x == typeof(OpenAction)).ShouldBeTrue();
            actions.Any(x => x == typeof(ContainingFolderAction)).ShouldBeTrue();
        }
    }
}