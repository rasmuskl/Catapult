using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class Launchable
{
    public IIndexable Action { get; set; }
    public IIndexable Target { get; set; }

    public Launchable(IIndexable action, IIndexable target)
    {
        Action = action;
        Target = target;
    }
}