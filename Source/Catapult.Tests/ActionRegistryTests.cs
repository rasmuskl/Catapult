using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Catapult.Core;
using Catapult.Core.Actions;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;
using NUnit.Framework;
using Should;

namespace Catapult.Tests
{
    [TestFixture]
    public class ActionRegistryTests
    {
        private ActionRegistry _actionRegistry;

        [SetUp]
        public void BeforeEachTest()
        {
            _actionRegistry = new ActionRegistry();

            _actionRegistry.RegisterAction<OpenAction>();
            _actionRegistry.RegisterAction<ContainingFolderConverter>();
            _actionRegistry.RegisterAction<GoogleAction>();
        }

        [Test]
        public void GetActionFor_FileItem()
        {
            ImmutableList<ActionMapping> actions = _actionRegistry.GetActionForInType(typeof(FileItem));

            actions.Count.ShouldEqual(1);

            actions.Any(x => x.ActionType == typeof(OpenAction)).ShouldBeTrue();
        }
        
        [Test]
        public void GetActionFor_GoogleAction()
        {
            ImmutableList<ActionMapping> types = _actionRegistry.GetTypesFor(typeof(GoogleAction));

            types.Count.ShouldEqual(1);
            types.Any(x => x.InType == typeof(StringIndexable)).ShouldBeTrue();
        }

        [Test]
        public void GetSearchFrame_GoogleAction()
        {
            ISearchFrame searchFrame = _actionRegistry.GetSearchFrame(new IIndexable[] { new GoogleAction() });

            searchFrame.ShouldBeType<StringSearchFrame>();
        }

        [Test]
        public void GetSearchFrame_FileItem()
        {
            ISearchFrame searchFrame = _actionRegistry.GetSearchFrame(new IIndexable[] { new FileItem(Assembly.GetExecutingAssembly().Location) });

            var indexableSearchFrame = searchFrame.ShouldBeType<IndexableSearchFrame>();

            indexableSearchFrame.Indexables.Length.ShouldEqual(2);
            indexableSearchFrame.Indexables.OfType<ContainingFolderConverter>().Count().ShouldEqual(1);
        }

        [Test]
        public void GetSearchFrame_FileItem_ContainingFolder()
        {
            ISearchFrame searchFrame = _actionRegistry.GetSearchFrame(new IIndexable[] { new FileItem(Assembly.GetExecutingAssembly().Location), new ContainingFolderConverter() });

            var indexableSearchFrame = searchFrame.ShouldBeType<IndexableSearchFrame>();

            indexableSearchFrame.Indexables.Length.ShouldEqual(2);
            indexableSearchFrame.Indexables.OfType<OpenAction>().Count().ShouldEqual(1);
            indexableSearchFrame.Indexables.OfType<ContainingFolderConverter>().Count().ShouldEqual(1);
        }

        [Test]
        public void Launch_FileItem()
        {
            Launchable launchable = _actionRegistry.Launch(new IIndexable[] { new FileItem(Assembly.GetExecutingAssembly().Location) });

            launchable.Action.ShouldBeType<OpenAction>();
            launchable.Target.ShouldBeType<FileItem>();
        }

        [Test]
        public void Launch_FileItem_ContainingFolder()
        {
            Launchable launchable = _actionRegistry.Launch(new IIndexable[] { new FileItem(Assembly.GetExecutingAssembly().Location), new ContainingFolderConverter() });

            launchable.Action.ShouldBeType<OpenAction>();
            launchable.Target.ShouldBeType<FolderItem>();
        }
    }
}