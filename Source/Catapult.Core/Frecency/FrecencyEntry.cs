namespace Catapult.Core.Frecency;

public class FrecencyEntry
{
    public string BoostIdentifier { get; set; }
    public string SearchString { get; set; }
    public int Index { get; set; }
    public DateTime UtcUse { get; set; }

    private FrecencyEntry()
    {
    }

    public FrecencyEntry(string boostIdentifier, string searchString, int index, DateTime utcUse)
    {
        BoostIdentifier = boostIdentifier;
        SearchString = searchString;
        Index = index;
        UtcUse = utcUse;
    }
}