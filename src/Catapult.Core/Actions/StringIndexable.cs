using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class StringIndexable(string name, string? details = null, IIconResolver? iconResolver = null)
    : IndexableBase
{
    public override string Name { get; } = name;
    public override string? Details { get; } = details;
    public override string BoostIdentifier => string.Empty;

    public override IIconResolver GetIconResolver()
    {
        return iconResolver ?? base.GetIconResolver();
    }
}