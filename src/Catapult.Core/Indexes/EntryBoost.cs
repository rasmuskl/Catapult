namespace Catapult.Core.Indexes;

public class EntryBoost(string boostIdentifier)
{
    public string BoostIdentifier { get; private set; } = boostIdentifier;
}