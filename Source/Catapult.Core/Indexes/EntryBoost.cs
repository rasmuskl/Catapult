namespace Catapult.Core.Indexes;

public class EntryBoost
{
    public string BoostIdentifier { get; private set; }

    public EntryBoost(string boostIdentifier)
    {
        BoostIdentifier = boostIdentifier;
    }
}