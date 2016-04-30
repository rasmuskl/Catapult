using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class StringIndexable : IndexableBase
    {
        public StringIndexable(string name)
        {
            Name = name;
        }

        public override string Name { get; }
    }
}