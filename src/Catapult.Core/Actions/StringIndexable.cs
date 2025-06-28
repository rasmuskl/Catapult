using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class StringIndexable : IndexableBase
{
    private readonly IIconResolver? _iconResolver;

    public StringIndexable(string name, string? details = null, IIconResolver? iconResolver = null)
    {
        _iconResolver = iconResolver;
        Details = details;
        Name = name;
    }

    public override string Name { get; }
    public override string? Details { get; }
    public override string BoostIdentifier => string.Empty;

    public override IIconResolver GetIconResolver()
    {
        return _iconResolver ?? base.GetIconResolver();
    }
}