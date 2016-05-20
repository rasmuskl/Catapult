using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class StringIndexable : IndexableBase
    {
        public StringIndexable(string name, string details = null)
        {
            Details = details;
            Name = name;
        }

        public override string Name { get; }
        public override string Details { get; }
        public override string BoostIdentifier => string.Empty;
    }
}