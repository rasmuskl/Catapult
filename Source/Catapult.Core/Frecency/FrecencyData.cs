namespace Catapult.Core.Frecency;

public class FrecencyData
{
    public List<FrecencyEntry> Entries { get; private set; } = new List<FrecencyEntry>();
     
    public void AddUse(string boostIdentifier, string searchString, int index)
    {
        Entries.Add(new FrecencyEntry(boostIdentifier, searchString, index, DateTime.UtcNow));
    }
}