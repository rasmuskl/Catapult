using Catapult.Core.Icons;

namespace Catapult.Core.Indexes;

public interface IIndexable
{
    string Name { get; }
    string? Details { get; }
    string BoostIdentifier { get; }
    object GetDetails();
    IIconResolver GetIconResolver();
}