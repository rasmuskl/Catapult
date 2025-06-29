using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class IndexableResult(IIndexable[] indexables, string state)
{
    public IIndexable[] Indexables { get; set; } = indexables;
    public string State { get; set; } = state;
}