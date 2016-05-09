using System;
using System.Collections.Immutable;
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

            ImmutableList<Type> actions = actionRegistry.GetActionFor(typeof(FileItem));

            actions.Count.ShouldEqual(2);

            actions.Any(x => x == typeof(OpenAction)).ShouldBeTrue();
            actions.Any(x => x == typeof(ContainingFolderAction)).ShouldBeTrue();
        }

        [Test]
        public void GetActionFor_GoogleAction()
        {
            var actionRegistry = new ActionRegistry();

            actionRegistry.RegisterAction<OpenAction>();
            actionRegistry.RegisterAction<ContainingFolderAction>();
            actionRegistry.RegisterAction<GoogleAction>();

            ImmutableList<Type> types = actionRegistry.GetTypesFor(typeof(GoogleAction));

            types.Count.ShouldEqual(1);
            types.Any(x => x == typeof(StringIndexable)).ShouldBeTrue();
        }

        [Test]
        public void GetSearchFrame_GoogleAction()
        {
            var actionRegistry = new ActionRegistry();

            actionRegistry.RegisterAction<OpenAction>();
            actionRegistry.RegisterAction<ContainingFolderAction>();
            actionRegistry.RegisterAction<GoogleAction>();

            ISearchFrame searchFrame = actionRegistry.GetSearchFrame(typeof(GoogleAction));

            searchFrame.ShouldBeType<StringSearchFrame>();
        }
    }
}