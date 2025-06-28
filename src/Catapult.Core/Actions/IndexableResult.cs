using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class IndexableResult
{
    public IIndexable[] Indexables { get; set; }
    public string State { get; set; }

    public IndexableResult(IIndexable[] indexables, string state)
    {
        Indexables = indexables;
        State = state;
    }
}