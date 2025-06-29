using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class Launchable(IIndexable action, IIndexable target)
{
    public IIndexable Action { get; set; } = action;
    public IIndexable Target { get; set; } = target;
}