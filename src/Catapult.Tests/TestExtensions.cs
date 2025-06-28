using Catapult.Core.Actions;
using Catapult.Core.Indexes;

namespace Catapult.Tests;

public static class TestExtensions
{
    public static IIndexable[] ToStringIndexables(this IEnumerable<string> strings)
    {
        return strings.Select(x => new StringIndexable(x)).OfType<IIndexable>().ToArray();
    }
}