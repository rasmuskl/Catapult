using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions;

public class ReindexFilesAction : IndexableBase, IStandaloneAction
{
    public void Run()
    {
        SearchResources.EnqueueDelayedIndexing(true);
    }

    public override string Name => "Catapult: Reindex files";
}