using Catapult.Core.Indexes;

namespace Catapult.Core.Config;

public class JsonIndexData
{
    public JsonIndexData()
    {
        BoostEntries = new Dictionary<string, EntryBoost>();
    }

    public Dictionary<string, EntryBoost> BoostEntries { get; set; } 
}