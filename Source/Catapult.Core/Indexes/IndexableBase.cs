using Catapult.Core.Icons;

namespace Catapult.Core.Indexes;

public abstract class IndexableBase : IIndexable
{
    public abstract string Name { get; }
    public virtual string? Details => null;
    public virtual string BoostIdentifier => $"{GetType().Name}-{Name}";

    public virtual object GetDetails()
    {
        return Name;
    }

    public virtual IIconResolver GetIconResolver()
    {
        return new EmptyIconResolver();
    }
}