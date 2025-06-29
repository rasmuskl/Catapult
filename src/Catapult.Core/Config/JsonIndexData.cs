using Catapult.Core.Indexes;

namespace Catapult.Core.Config;

public class JsonIndexData
{
    public Dictionary<string, EntryBoost> BoostEntries { get; set; } = new();
}